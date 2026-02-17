import { Component, OnInit } from '@angular/core';
import { Router } from '@angular/router';
import { StoreService } from '../../services/store.service';
import { ProductService } from '../../services/product.service';
import { AuthService } from '../../services/auth.service';

@Component({
  selector: 'app-superadmin',
  standalone: false,
  templateUrl: './superadmin.component.html',
  styleUrl: './superadmin.component.css'
})
export class SuperadminComponent implements OnInit {
  stats = {
    totalStores: 0,
    totalProducts: 0,
    totalCategories: 0,
    totalAssignments: 0
  };

  recentStores: any[] = [];
  recentProducts: any[] = [];
  loading = true;
  currentYear = new Date().getFullYear();
  userName: string = '';

  // Chart data
  monthlyStats = {
    labels: ['Jan', 'Feb', 'Mar', 'Apr', 'May', 'Jun'],
    stores: [5, 8, 12, 15, 18, 22],
    products: [45, 52, 68, 85, 102, 128]
  };

  constructor(
    private storeService: StoreService,
    private productService: ProductService,
    private authService: AuthService,
    private router: Router
  ) {
    this.userName = this.authService.getFullName() || 'Admin';
  }

  ngOnInit(): void {
    this.loadDashboardData();
  }

  loadDashboardData(): void {
    this.loading = true;

    // Get all stores
    this.storeService.getStores().subscribe({
      next: (stores: any) => {
        this.stats.totalStores = stores.length;
        this.recentStores = stores.slice(0, 5);
      },
      error: (err) => console.error('Error loading stores:', err)
    });

    // Get all products
    this.productService.getProducts().subscribe({
      next: (products: any) => {
        this.stats.totalProducts = products.length;
        this.recentProducts = products.slice(0, 5);
      },
      error: (err) => console.error('Error loading products:', err)
    });

    // Get all categories
    this.productService.getCategories().subscribe({
      next: (categories: any) => {
        this.stats.totalCategories = categories.length;
      },
      error: (err) => console.error('Error loading categories:', err)
    });

    // Get all assignments
    this.productService.getAssignments().subscribe({
      next: (assignments: any) => {
        this.stats.totalAssignments = assignments.length;
        this.loading = false;
      },
      error: (err) => {
        console.error('Error loading assignments:', err);
        this.loading = false;
      }
    });
  }

  logout(): void {
    this.authService.logout();
  }

  navigateTo(route: string): void {
    this.router.navigate([`/admin/${route}`]);
  }
}
