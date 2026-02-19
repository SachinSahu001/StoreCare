import { Component, OnInit, ViewChild } from '@angular/core';
import { MatTableDataSource } from '@angular/material/table';
import { MatPaginator } from '@angular/material/paginator';
import { MatSort } from '@angular/material/sort';
import { MatDialog } from '@angular/material/dialog';
import { MatSnackBar } from '@angular/material/snack-bar';
import { ProductService, ProductCategory } from '../../../core/services/product.service';
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
        const formData = new FormData();
        formData.append('CategoryName', result.categoryName);
        if (result.categoryDescription) {
          formData.append('CategoryDescription', result.categoryDescription);
        }
        formData.append('DisplayOrder', result.displayOrder.toString());

        // Handle Active status if needed - currently backend defaults to Active on create.
        // For update, we might need StatusId, but we don't have the ID map here easily.
        // For now, we rely on the implementation plan to fix core fields first.

        if (result.file) {
          formData.append('CategoryImage', result.file);
        }

        if (category) {
          // For updates, we might want to send StatusId if user changed valid active/inactive
          // But let's stick to core fields for now as per user request
          this.updateCategory(category.id, formData);
        } else {
          this.createCategory(formData);
        }
      }
    });
  }

  createCategory(category: FormData): void {
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

  updateCategory(id: string, category: FormData): void {
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
