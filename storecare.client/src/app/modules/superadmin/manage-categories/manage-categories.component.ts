import { Component, OnInit, ViewChild } from '@angular/core';
import { MatTableDataSource } from '@angular/material/table';
import { MatPaginator } from '@angular/material/paginator';
import { MatSort } from '@angular/material/sort';
import { MatDialog } from '@angular/material/dialog';
import { MatSnackBar } from '@angular/material/snack-bar';
import { ProductService, ProductCategory } from '../../../services/product.service';
import { CategoryDialogComponent } from './category-dialog/category-dialog.component';

@Component({
  selector: 'app-manage-categories',
  standalone: false,
  templateUrl: './manage-categories.component.html',
  styleUrl: './manage-categories.component.css'
})
export class ManageCategoriesComponent implements OnInit {
  displayedColumns: string[] = ['imageUrl', 'categoryName', 'categoryCode', 'displayOrder', 'status', 'actions'];
  dataSource!: MatTableDataSource<ProductCategory>;
  isLoading = true;

  @ViewChild(MatPaginator) paginator!: MatPaginator;
  @ViewChild(MatSort) sort!: MatSort;

  constructor(
    private productService: ProductService,
    private dialog: MatDialog,
    private snackBar: MatSnackBar
  ) { }

  ngOnInit(): void {
    this.loadCategories();
  }

  loadCategories(): void {
    this.isLoading = true;
    this.productService.getCategories().subscribe({
      next: (data) => {
        this.dataSource = new MatTableDataSource(data);
        this.dataSource.paginator = this.paginator;
        this.dataSource.sort = this.sort;
        this.isLoading = false;
      },
      error: (error) => {
        console.error('Error loading categories', error);
        this.showSnackBar('Error loading categories', 'Close');
        this.isLoading = false;
      }
    });
  }

  applyFilter(event: Event): void {
    const filterValue = (event.target as HTMLInputElement).value;
    this.dataSource.filter = filterValue.trim().toLowerCase();

    if (this.dataSource.paginator) {
      this.dataSource.paginator.firstPage();
    }
  }

  openDialog(category?: ProductCategory): void {
    const dialogRef = this.dialog.open(CategoryDialogComponent, {
      width: '500px',
      data: category || {}
    });

    dialogRef.afterClosed().subscribe(result => {
      if (result) {
        if (category) {
          // Check if result has changed
          // For now assume changed
          this.updateCategory(category.id, result);
        } else {
          this.createCategory(result);
        }
      }
    });
  }

  createCategory(category: Partial<ProductCategory>): void {
    this.productService.createCategory(category).subscribe({
      next: () => {
        this.showSnackBar('Category created successfully', 'Close');
        this.loadCategories();
      },
      error: (error) => {
        console.error('Error creating category', error);
        this.showSnackBar('Error creating category', 'Close');
      }
    });
  }

  updateCategory(id: string, category: Partial<ProductCategory>): void {
    this.productService.updateCategory(id, category).subscribe({
      next: () => {
        this.showSnackBar('Category updated successfully', 'Close');
        this.loadCategories();
      },
      error: (error) => {
        console.error('Error updating category', error);
        this.showSnackBar('Error updating category', 'Close');
      }
    });
  }

  deleteCategory(id: string): void {
    if (confirm('Are you sure you want to delete this category?')) {
      this.productService.deleteCategory(id).subscribe({
        next: () => {
          this.showSnackBar('Category deleted successfully', 'Close');
          this.loadCategories();
        },
        error: (error) => {
          console.error('Error deleting category', error);
          this.showSnackBar('Error deleting category', 'Close');
        }
      });
    }
  }

  showSnackBar(message: string, action: string): void {
    this.snackBar.open(message, action, {
      duration: 3000,
      horizontalPosition: 'end',
      verticalPosition: 'top'
    });
  }
}
