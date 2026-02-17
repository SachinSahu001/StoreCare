import { Component, OnInit } from '@angular/core';
import { AuthService } from '../../services/auth.service';

@Component({
  selector: 'app-sidebar',
  standalone: false,
  templateUrl: './sidebar.component.html',
  styleUrls: ['./sidebar.component.css']
})
export class SidebarComponent implements OnInit {
  userRole: string | null = null;
  menuItems: any[] = [];

  // Make authService public so it can be accessed in the template
  constructor(public authService: AuthService) { }

  ngOnInit(): void {
    this.userRole = this.authService.getRole();
    this.loadMenuItems();
  }

  loadMenuItems(): void {
    // Common menu items for all users
    const commonItems = [
      { label: 'Dashboard', icon: 'fas fa-home', route: './' },
      { label: 'Profile', icon: 'fas fa-user', route: 'profile' }
    ];

    // Role-specific menu items
    if (this.userRole === 'SuperAdmin') {
      this.menuItems = [
        ...commonItems,
        { label: 'Users', icon: 'fas fa-users', route: 'users' },
        { label: 'Stores', icon: 'fas fa-store', route: 'stores' },
        { label: 'Settings', icon: 'fas fa-cog', route: 'settings' },
        { label: 'Reports', icon: 'fas fa-chart-bar', route: 'reports' }
      ];
    } else if (this.userRole === 'StoreAdmin') {
      this.menuItems = [
        ...commonItems,
        { label: 'Products', icon: 'fas fa-box', route: 'products' },
        { label: 'Orders', icon: 'fas fa-shopping-cart', route: 'orders' },
        { label: 'Inventory', icon: 'fas fa-warehouse', route: 'inventory' },
        { label: 'Customers', icon: 'fas fa-users', route: 'customers' }
      ];
    } else if (this.userRole === 'Customer') {
      this.menuItems = [
        ...commonItems,
        { label: 'My Orders', icon: 'fas fa-shopping-bag', route: 'orders' },
        { label: 'Wishlist', icon: 'fas fa-heart', route: 'wishlist' },
        { label: 'Addresses', icon: 'fas fa-address-book', route: 'addresses' },
        { label: 'Payments', icon: 'fas fa-credit-card', route: 'payments' }
      ];
    }
  }
}
