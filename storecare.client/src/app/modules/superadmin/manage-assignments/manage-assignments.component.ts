import { Component, OnInit, ViewChild } from '@angular/core';
import { MatTableDataSource } from '@angular/material/table';
import { MatPaginator } from '@angular/material/paginator';
import { MatSort } from '@angular/material/sort';
import { MatSnackBar } from '@angular/material/snack-bar';
import { SelectionModel } from '@angular/cdk/collections';
import { ProductService, Product, ProductCategory, StoreProductAssignment } from '../../../services/product.service';
import { StoreService, Store } from '../../../services/store.service';

@Component({
  selector: 'app-manage-assignments',
  standalone: false,
  templateUrl: './manage-assignments.component.html',
  styleUrl: './manage-assignments.component.css'
})
export class ManageAssignmentsComponent implements OnInit {
  displayedColumns: string[] = ['select', 'imageUrl', 'productName', 'productCode', 'stockQuantity'];
  dataSource!: MatTableDataSource<Product>;
  selection = new SelectionModel<Product>(true, []);

  categories: ProductCategory[] = [];
  stores: Store[] = [];

  selectedCategoryId: string = '';
  selectedStoreId: string = '';

  isLoading = false;
  existingAssignments: Set<string> = new Set(); // Set of Product IDs already assigned to selected store

  @ViewChild(MatPaginator) paginator!: MatPaginator;
  @ViewChild(MatSort) sort!: MatSort;

  constructor(
    private productService: ProductService,
    private storeService: StoreService,
    private snackBar: MatSnackBar
  ) { }

  ngOnInit(): void {
    this.loadCategories();
    this.loadStores();
  }

  loadCategories(): void {
    this.productService.getCategories().subscribe(data => this.categories = data);
  }

  loadStores(): void {
    this.storeService.getStores().subscribe(data => this.stores = data);
  }

  onCategoryChange(): void {
    if (this.selectedCategoryId) {
      this.loadProducts();
    } else {
      if (this.dataSource) this.dataSource.data = [];
    }
  }

  onStoreChange(): void {
    if (this.selectedStoreId) {
      this.loadExistingAssignments();
    } else {
      this.existingAssignments.clear();
    }
  }

  loadExistingAssignments(): void {
    if (!this.selectedStoreId) return;

    this.productService.getAssignmentsByStore(this.selectedStoreId).subscribe({
      next: (assignments) => {
        this.existingAssignments = new Set(assignments.map(a => a.productId));
        // Refresh table to update disabled states if needed (though Angular change detection handles this in template)
      },
      error: (error) => console.error('Error loading assignments', error)
    });
  }

  loadProducts(): void {
    if (!this.selectedCategoryId) return;

    this.isLoading = true;
    this.productService.getProductsByCategory(this.selectedCategoryId).subscribe({
      next: (data) => {
        this.dataSource = new MatTableDataSource(data);
        this.dataSource.paginator = this.paginator;
        this.dataSource.sort = this.sort;
        this.selection.clear();
        this.isLoading = false;
      },
      error: (error) => {
        console.error('Error loading products', error);
        this.snackBar.open('Error loading products', 'Close', { duration: 3000 });
        this.isLoading = false;
      }
    });
  }

  /** Whether the number of selected elements matches the total number of rows. */
  isAllSelected() {
    if (!this.dataSource) return false;
    const numSelected = this.selection.selected.length;
    const numRows = this.dataSource.data.filter(row => !this.isAssigned(row.id)).length;
    return numSelected === numRows && numRows > 0;
  }

  /** Selects all rows if they are not all selected; otherwise clear selection. */
  toggleAllRows() {
    if (this.isAllSelected()) {
      this.selection.clear();
      return;
    }

    this.dataSource.data.forEach(row => {
      if (!this.isAssigned(row.id)) this.selection.select(row);
    });
  }

  isAssigned(productId: string): boolean {
    return this.existingAssignments.has(productId);
  }

  assignSelected(): void {
    if (!this.selectedStoreId || this.selection.isEmpty()) return;

    const productsToAssign = this.selection.selected;
    let completed = 0;
    let errors = 0;

    this.isLoading = true;

    // Sequential or Parallel requests? Parallel is faster.
    // Ideally backend should have a bulk endpoint. Since not specified, I loop.
    // "Manage Assignments page must allow bulk and single assignment: first select category → load products filtered by category → select store → assign selected products in bulk via checkboxes and submit"

    // I'll use a simple loop.
    productsToAssign.forEach(product => {
      const assignment: Partial<StoreProductAssignment> = {
        storeId: this.selectedStoreId,
        productId: product.id,
        sellingPrice: product.price, // Default to product price
        stockQuantity: 0, // Initial stock 0
        minStockLevel: 5,
        maxStockLevel: 100,
        reorderLevel: 10,
        statusId: 1,
        active: true
      };

      this.productService.createAssignment(assignment).subscribe({
        next: () => {
          completed++;
          this.checkCompletion(productsToAssign.length, completed, errors);
        },
        error: () => {
          errors++;
          this.checkCompletion(productsToAssign.length, completed, errors);
        }
      });
    });
  }

  checkCompletion(total: number, completed: number, errors: number): void {
    if (completed + errors === total) {
      this.isLoading = false;
      this.snackBar.open(`Assigned ${completed} products. Failed: ${errors}`, 'Close', { duration: 3000 });
      this.loadExistingAssignments(); // Refresh existing assignments
      this.selection.clear();
    }
  }
}
