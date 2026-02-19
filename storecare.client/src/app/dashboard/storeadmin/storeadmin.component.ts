import { Component, OnInit } from '@angular/core';
import { Router } from '@angular/router';
import { StoreService } from '../../services/store.service';
import { ProductService } from '../../core/services/product.service';
import { AuthService } from '../../services/auth.service';

@Component({
  selector: 'app-storeadmin',
  standalone: false,
  templateUrl: './storeadmin.component.html',
  styleUrl: './storeadmin.component.css'
})
export class StoreadminComponent implements OnInit {
  store: any = null;
  assignedProducts: any[] = [];
  stats = {
    totalProducts: 0,
    lowStock: 0,
    outOfStock: 0,
    totalValue: 0
  };
  loading = true;
  currentYear = new Date().getFullYear();
  userName: string = '';

  constructor(
    private storeService: StoreService,
    private productService: ProductService,
    private authService: AuthService,
    private router: Router
  ) {
    this.userName = this.authService.getFullName() || 'Store Admin';
  }

  ngOnInit(): void {
    this.loadStoreData();
  }

  loadStoreData(): void {
    this.loading = true;

    // Get store details
    this.storeService.getOwnStore().subscribe({
      next: (store) => {
        this.store = store;
      },
      error: (err) => console.error('Error loading store:', err)
    });

    // Get assigned products
    this.productService.getAssignedProducts().subscribe({
      next: (assignments: any) => {
        this.assignedProducts = assignments;
        this.calculateStats(assignments);
        this.loading = false;
      },
      error: (err) => {
        console.error('Error loading assignments:', err);
        this.loading = false;
      }
    });
  }

  calculateStats(assignments: any[]): void {
    this.stats.totalProducts = assignments.length;
    this.stats.lowStock = assignments.filter(a => a.stockQuantity <= a.minStockLevel).length;
    this.stats.outOfStock = assignments.filter(a => a.stockQuantity === 0).length;
    this.stats.totalValue = assignments.reduce((sum, a) => sum + (a.sellingPrice * a.stockQuantity), 0);
  }

  logout(): void {
    this.authService.logout();
  }

  navigateTo(route: string): void {
    this.router.navigate([`/dashboard/storeadmin/${route}`]);
  }

  getStockStatus(quantity: number, minLevel: number): string {
    if (quantity === 0) return 'out';
    if (quantity <= minLevel) return 'low';
    return 'good';
  }

  getStockStatusText(quantity: number, minLevel: number): string {
    if (quantity === 0) return 'Out of Stock';
    if (quantity <= minLevel) return 'Low Stock';
    return 'In Stock';
  }
}
