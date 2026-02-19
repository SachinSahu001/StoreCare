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
        { label: 'Dashboard', icon: 'fas fa-home', route: '/dashboard/superadmin/dashboard' },
        { label: 'Categories', icon: 'fas fa-tags', route: '/dashboard/superadmin/categories' },
        { label: 'Products', icon: 'fas fa-box-open', route: '/dashboard/superadmin/products' },
        { label: 'Stores', icon: 'fas fa-store', route: '/dashboard/superadmin/stores' },
        { label: 'Assignments', icon: 'fas fa-dolly-flatbed', route: '/dashboard/superadmin/assignments' },
        { label: 'Users', icon: 'fas fa-users', route: '/dashboard/superadmin/users' },
      ];
    } else if (this.userRole === 'StoreAdmin') {
      this.menuItems = [
        ...commonItems,
        { label: 'Products', icon: 'fas fa-box', route: '/dashboard/storeadmin/products' },
        { label: 'Orders', icon: 'fas fa-shopping-cart', route: '/dashboard/storeadmin/orders' },
        { label: 'Inventory', icon: 'fas fa-warehouse', route: '/dashboard/storeadmin/inventory' },
        { label: 'Customers', icon: 'fas fa-users', route: '/dashboard/storeadmin/customers' }
      ];
    } else if (this.userRole === 'Customer') {
      this.menuItems = [
        ...commonItems,
        { label: 'My Orders', icon: 'fas fa-shopping-bag', route: '/dashboard/customer/orders' },
        { label: 'Wishlist', icon: 'fas fa-heart', route: '/dashboard/customer/wishlist' },
        { label: 'Addresses', icon: 'fas fa-address-book', route: '/dashboard/customer/addresses' },
        { label: 'Payments', icon: 'fas fa-credit-card', route: '/dashboard/customer/payments' }
      ];
    }
  }
}
