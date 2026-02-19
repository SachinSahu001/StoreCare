import { Component, OnInit, ViewChild } from '@angular/core';
import { MatTableDataSource } from '@angular/material/table';
import { MatPaginator } from '@angular/material/paginator';
import { MatSort } from '@angular/material/sort';
import { MatDialog } from '@angular/material/dialog';
import { MatSnackBar } from '@angular/material/snack-bar';
import { ProductService, Product, ProductCategory } from '../../../core/services/product.service';
import { ProductDialogComponent } from './product-dialog/product-dialog.component';

@Component({
  selector: 'app-manage-products',
  standalone: false,
  templateUrl: './manage-products.component.html',
  styleUrl: './manage-products.component.css'
})
export class ManageProductsComponent implements OnInit {
  displayedColumns: string[] = ['imageUrl', 'productName', 'productCode', 'categoryName', 'price', 'stockQuantity', 'status', 'actions'];
  dataSource!: MatTableDataSource<Product>;
  isLoading = true;
  categories: ProductCategory[] = [];

  @ViewChild(MatPaginator) paginator!: MatPaginator;
  @ViewChild(MatSort) sort!: MatSort;

  constructor(
    private productService: ProductService,
    private dialog: MatDialog,
    private snackBar: MatSnackBar
  ) { }

  ngOnInit(): void {
    this.loadProducts();
    this.loadCategories();
  }

  loadProducts(): void {
    this.isLoading = true;
    this.productService.getProducts().subscribe({
      next: (data: Product[]) => {
        this.dataSource = new MatTableDataSource(data);
        this.dataSource.paginator = this.paginator;
        this.dataSource.sort = this.sort;
        this.isLoading = false;
      },
      error: (error: any) => {
        console.error('Error loading products', error);
        this.showSnackBar('Error loading products', 'Close');
        this.isLoading = false;
      }
    });
  }

  loadCategories(): void {
    this.productService.getCategories().subscribe({
      next: (data: ProductCategory[]) => {
        this.categories = data;
      },
      error: (error: any) => console.error('Error loading categories', error)
    });
  }

  applyFilter(event: Event): void {
    const filterValue = (event.target as HTMLInputElement).value;
    this.dataSource.filter = filterValue.trim().toLowerCase();

    if (this.dataSource.paginator) {
      this.dataSource.paginator.firstPage();
    }
  }

  openDialog(product?: Product): void {
    const dialogRef = this.dialog.open(ProductDialogComponent, {
      width: '600px',
      data: { product: product || {}, categories: this.categories }
    });

    dialogRef.afterClosed().subscribe(result => {
      if (result) {
        const formData = new FormData();
        formData.append('ProductName', result.productName);
        formData.append('CategoryId', result.categoryId);
        formData.append('BrandName', result.brandName);
        if (result.model) formData.append('Model', result.model);
        if (result.productDescription) formData.append('ProductDescription', result.productDescription);

        formData.append('Price', result.price.toString());
        formData.append('Mrp', result.mrp?.toString() || '0');
        formData.append('StockQuantity', result.stockQuantity.toString());
        formData.append('Unit', result.unit);
        formData.append('IsFeatured', String(result.isFeatured));

        // Active status handling if needed for updates via StatusId

        if (result.file) {
          formData.append('ProductImage', result.file);
        }

        if (product) {
          this.updateProduct(product.id, formData);
        } else {
          this.createProduct(formData);
        }
      }
    });
  }

  createProduct(product: FormData): void {
    this.productService.createProduct(product).subscribe({
      next: () => {
        this.showSnackBar('Product created successfully', 'Close');
        this.loadProducts();
      },
      error: (error: any) => {
        console.error('Error creating product', error);
        this.showSnackBar('Error creating product', 'Close');
      }
    });
  }

  updateProduct(id: string, product: FormData): void {
    this.productService.updateProduct(id, product).subscribe({
      next: () => {
        this.showSnackBar('Product updated successfully', 'Close');
        this.loadProducts();
      },
      error: (error: any) => {
        console.error('Error updating product', error);
        this.showSnackBar('Error updating product', 'Close');
      }
    });
  }

  deleteProduct(id: string): void {
    if (confirm('Are you sure you want to delete this product?')) {
      this.productService.deleteProduct(id).subscribe({
        next: () => {
          this.showSnackBar('Product deleted successfully', 'Close');
          this.loadProducts();
        },
        error: (error: any) => {
          console.error('Error deleting product', error);
          this.showSnackBar('Error deleting product', 'Close');
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
