import { Component, OnInit } from '@angular/core';
import { Router } from '@angular/router';
import { ProductService } from '../../services/product.service';
import { AuthService } from '../../services/auth.service';

@Component({
  selector: 'app-customer',
  standalone: false,
  templateUrl: './customer.component.html',
  styleUrl: './customer.component.css'
})
export class CustomerComponent implements OnInit {
  categories: any[] = [];
  featuredProducts: any[] = [];
  recentProducts: any[] = [];
  loading = true;
  currentYear = new Date().getFullYear();
  userName: string = '';

  constructor(
    private productService: ProductService,
    private authService: AuthService,
    private router: Router
  ) {
    this.userName = this.authService.getFullName() || 'Customer';
  }

  ngOnInit(): void {
    this.loadDashboardData();
  }

  loadDashboardData(): void {
    this.loading = true;

    // Get categories
    this.productService.getCategories().subscribe({
      next: (categories: any) => {
        this.categories = categories.slice(0, 6);
      },
      error: (err) => console.error('Error loading categories:', err)
    });

    // Get products
    this.productService.getProducts().subscribe({
      next: (products: any) => {
        this.featuredProducts = products.filter((p: any) => p.isFeatured).slice(0, 4);
        this.recentProducts = products.slice(0, 8);
        this.loading = false;
      },
      error: (err) => {
        console.error('Error loading products:', err);
        this.loading = false;
      }
    });
  }

  logout(): void {
    this.authService.logout();
  }

  navigateTo(route: string): void {
    this.router.navigate([`/${route}`]);
  }

  viewProduct(productId: string): void {
    this.router.navigate(['/product', productId]);
  }

  viewCategory(categoryId: string): void {
    this.router.navigate(['/products'], { queryParams: { category: categoryId } });
  }

  addToCart(product: any): void {
    console.log('Adding to cart:', product);
    // Implement cart functionality
  }

  addToWishlist(product: any): void {
    console.log('Adding to wishlist:', product);
    // Implement wishlist functionality
  }
}
