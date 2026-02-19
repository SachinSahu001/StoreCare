import { Component, OnInit, ViewChild } from '@angular/core';
import { MatTableDataSource } from '@angular/material/table';
import { MatPaginator } from '@angular/material/paginator';
import { MatSort } from '@angular/material/sort';
import { MatSnackBar } from '@angular/material/snack-bar';
import { SelectionModel } from '@angular/cdk/collections';
import { ProductService, ProductCategory, StoreProductAssignment } from '../../../core/services/product.service';
import { StoreService, Store } from '../../../services/store.service';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';

@Component({
  selector: 'app-manage-assignments',
  standalone: false,
  templateUrl: './manage-assignments.component.html',
  styleUrl: './manage-assignments.component.css'
})
export class ManageAssignmentsComponent implements OnInit {
  // Tabs
  selectedTab = 0; // 0: Manage List, 1: Create New

  // List View
  displayedColumns: string[] = ['storeName', 'productName', 'categoryName', 'status', 'actions'];
  dataSource!: MatTableDataSource<StoreProductAssignment>;
  @ViewChild(MatPaginator) paginator!: MatPaginator;
  @ViewChild(MatSort) sort!: MatSort;

  // Create View
  assignmentForm: FormGroup;
  stores: Store[] = [];
  categories: ProductCategory[] = [];
  availableProducts: any[] = [];
  productSelection = new SelectionModel<any>(true, []);
  isStep2Loading = false;

  isLoading = false;

  constructor(
    private productService: ProductService,
    private storeService: StoreService,
    private snackBar: MatSnackBar,
    private fb: FormBuilder
  ) {
    this.assignmentForm = this.fb.group({
      storeId: ['', Validators.required],
      categoryId: ['', Validators.required]
    });
  }

  ngOnInit(): void {
    this.loadAssignments();
    this.loadStores();
    this.loadCategories();
  }

  // ================== LIST VIEW ==================
  loadAssignments(): void {
    this.isLoading = true;
    this.productService.getAssignments().subscribe({
      next: (data) => {
        this.dataSource = new MatTableDataSource(data);
        this.dataSource.paginator = this.paginator;
        this.dataSource.sort = this.sort;
        this.isLoading = false;
      },
      error: (err) => {
        console.error(err);
        this.isLoading = false;
      }
    });
  }

  deleteAssignment(id: string): void {
    if (confirm('Are you sure you want to remove this assignment?')) {
      this.productService.deleteAssignment(id).subscribe(() => {
        this.showSnackBar('Assignment removed');
        this.loadAssignments();
      });
    }
  }

  // ================== CREATE VIEW ==================
  loadStores(): void {
    this.storeService.getStores().subscribe(data => this.stores = data);
  }

  loadCategories(): void {
    this.productService.getCategories().subscribe(data => this.categories = data);
  }

  onStep1Next(): void {
    if (this.assignmentForm.valid) {
      this.loadProductsForAssignment();
    }
  }

  loadProductsForAssignment(): void {
    const { storeId, categoryId } = this.assignmentForm.value;
    this.isStep2Loading = true;
    this.productSelection.clear();

    this.productService.getProductsByCategoryForAssignment(categoryId, storeId).subscribe({
      next: (response) => {
        this.availableProducts = response.data; // Response data likely contains { id, productName, ... isAssigned }
        this.isStep2Loading = false;
      },
      error: (err) => {
        console.error(err);
        this.showSnackBar('Failed to load products');
        this.isStep2Loading = false;
      }
    });
  }

  // Toggle selection
  toggleProduct(product: any): void {
    if (product.isAssigned) return;
    this.productSelection.toggle(product);
  }

  isAllSelected(): boolean {
    const numSelected = this.productSelection.selected.length;
    const numRows = this.availableProducts.filter(p => !p.isAssigned).length;
    return numSelected === numRows && numRows > 0;
  }

  toggleAll(): void {
    if (this.isAllSelected()) {
      this.productSelection.clear();
    } else {
      this.availableProducts.forEach(p => {
        if (!p.isAssigned) this.productSelection.select(p);
      });
    }
  }

  submitAssignment(): void {
    const { storeId, categoryId } = this.assignmentForm.value;
    const productIds = this.productSelection.selected.map(p => p.id);

    if (productIds.length === 0) return;

    this.isLoading = true;
    const payload = {
      storeId,
      categoryId,
      productIds,
      canManage: true
    };

    this.productService.assignProductsByCategory(payload).subscribe({
      next: (res) => {
        this.showSnackBar(res.message);
        this.isLoading = false;
        this.assignmentForm.reset();
        this.productSelection.clear();
        this.availableProducts = [];
        this.selectedTab = 0; // Go back to list
        this.loadAssignments();
      },
      error: (err) => {
        console.error(err);
        this.showSnackBar('Assignment failed');
        this.isLoading = false;
      }
    });
  }

  showSnackBar(msg: string): void {
    this.snackBar.open(msg, 'Close', { duration: 3000 });
  }

  filteredProducts() {
    return this.availableProducts;
  }
}
