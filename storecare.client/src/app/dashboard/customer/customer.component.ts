import { Component, OnInit } from '@angular/core';
import { Router } from '@angular/router';
import { ProductService } from '../../core/services/product.service';
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

  // Local state management
  private cartItems: any[] = [];
  private wishlistItems: any[] = [];
  cartCount: number = 0;
  wishlistCount: number = 0;

  constructor(
    private productService: ProductService,
    private authService: AuthService,
    private router: Router
  ) {
    this.userName = this.authService.getFullName() || 'Customer';
    this.loadLocalStorageData();
  }

  ngOnInit(): void {
    this.loadDashboardData();
  }

  // Load data from localStorage
  private loadLocalStorageData(): void {
    try {
      // Load cart items
      const savedCart = localStorage.getItem('cartItems');
      if (savedCart) {
        this.cartItems = JSON.parse(savedCart);
        this.cartCount = this.cartItems.reduce((total, item) => total + (item.quantity || 1), 0);
      }

      // Load wishlist items
      const savedWishlist = localStorage.getItem('wishlistItems');
      if (savedWishlist) {
        this.wishlistItems = JSON.parse(savedWishlist);
        this.wishlistCount = this.wishlistItems.length;
      }

      // Load recent products
      const savedRecent = localStorage.getItem('recentProducts');
      if (savedRecent) {
        this.recentProducts = JSON.parse(savedRecent);
      }
    } catch (error) {
      console.error('Error loading localStorage data:', error);
    }
  }

  // Save data to localStorage
  private saveToLocalStorage(key: string, data: any): void {
    try {
      localStorage.setItem(key, JSON.stringify(data));
    } catch (error) {
      console.error(`Error saving to localStorage (${key}):`, error);
    }
  }

  loadDashboardData(): void {
    this.loading = true;

    // Get categories
    this.productService.getCategories().subscribe({
      next: (categories: any) => {
        // Add icons and colors to categories for better UI
        this.categories = categories.slice(0, 6).map((cat: any, index: number) => ({
          ...cat,
          icon: this.getCategoryIcon(index),
          color: this.getCategoryColor(index)
        }));
      },
      error: (err) => {
        console.error('Error loading categories:', err);
        // Load fallback categories
        this.categories = this.getFallbackCategories();
      }
    });

    // Get products
    this.productService.getProducts().subscribe({
      next: (products: any) => {
        // Process featured products
        this.featuredProducts = products
          .filter((p: any) => p.isFeatured)
          .slice(0, 4)
          .map((product: any) => this.enhanceProductData(product));

        // Process recent products (combine with localStorage recent items)
        const newProducts = products.slice(0, 8).map((product: any) => this.enhanceProductData(product));

        // Merge with existing recent products, avoiding duplicates
        const existingIds = new Set(this.recentProducts.map(p => p.id));
        const uniqueNewProducts = newProducts.filter((p: any) => !existingIds.has(p.id));
        this.recentProducts = [...uniqueNewProducts, ...this.recentProducts].slice(0, 8);

        // Save recent products to localStorage
        this.saveToLocalStorage('recentProducts', this.recentProducts);

        this.loading = false;
      },
      error: (err) => {
        console.error('Error loading products:', err);
        // Load fallback products
        this.featuredProducts = this.getFallbackFeaturedProducts();
        if (this.recentProducts.length === 0) {
          this.recentProducts = this.getFallbackRecentProducts();
        }
        this.loading = false;
      }
    });
  }

  // Helper method to enhance product data
  private enhanceProductData(product: any): any {
    return {
      ...product,
      // Add random discount for some products
      discount: Math.random() > 0.7 ? Math.floor(Math.random() * 30) + 10 : null,
      originalPrice: product.price ? (product.price * 1.2).toFixed(2) : null,
      // Add random reviews count
      reviews: Math.floor(Math.random() * 50) + 10,
      // Ensure image URL
      imageUrl: product.imageUrl || `https://via.placeholder.com/300x300?text=${product.productName?.replace(' ', '+') || 'Product'}`
    };
  }

  // Get category icon based on index
  private getCategoryIcon(index: number): string {
    const icons = [
      'fa-mobile-alt', 'fa-laptop', 'fa-tshirt',
      'fa-shoe-prints', 'fa-couch', 'fa-book',
      'fa-gamepad', 'fa-watch', 'fa-camera',
      'fa-car', 'fa-utensils', 'fa-gem'
    ];
    return icons[index % icons.length];
  }

  // Get category color based on index
  private getCategoryColor(index: number): string {
    const colors = [
      'linear-gradient(135deg, #667eea 0%, #764ba2 100%)',
      'linear-gradient(135deg, #f093fb 0%, #f5576c 100%)',
      'linear-gradient(135deg, #4facfe 0%, #00f2fe 100%)',
      'linear-gradient(135deg, #43e97b 0%, #38f9d7 100%)',
      'linear-gradient(135deg, #fa709a 0%, #fee140 100%)',
      'linear-gradient(135deg, #30cfd0 0%, #330867 100%)'
    ];
    return colors[index % colors.length];
  }

  // Fallback categories if API fails
  private getFallbackCategories(): any[] {
    return [
      { id: '1', categoryName: 'Electronics', description: 'Gadgets & devices', icon: 'fa-mobile-alt' },
      { id: '2', categoryName: 'Fashion', description: 'Clothing & apparel', icon: 'fa-tshirt' },
      { id: '3', categoryName: 'Home & Living', description: 'Furniture & decor', icon: 'fa-couch' },
      { id: '4', categoryName: 'Books', description: 'Books & media', icon: 'fa-book' },
      { id: '5', categoryName: 'Sports', description: 'Sports & outdoors', icon: 'fa-bicycle' },
      { id: '6', categoryName: 'Toys', description: 'Games & toys', icon: 'fa-gamepad' }
    ].map((cat, index) => ({
      ...cat,
      color: this.getCategoryColor(index)
    }));
  }

  // Fallback featured products
  private getFallbackFeaturedProducts(): any[] {
    return [
      {
        id: '1',
        productName: 'Wireless Headphones',
        price: '99.99',
        originalPrice: '129.99',
        discount: 23,
        imageUrl: 'https://via.placeholder.com/300x300?text=Headphones',
        isFeatured: true,
        reviews: 45
      },
      {
        id: '2',
        productName: 'Smart Watch',
        price: '199.99',
        originalPrice: '249.99',
        discount: 20,
        imageUrl: 'https://via.placeholder.com/300x300?text=Smart+Watch',
        isFeatured: true,
        reviews: 32
      },
      {
        id: '3',
        productName: 'Laptop Backpack',
        price: '49.99',
        originalPrice: null,
        discount: null,
        imageUrl: 'https://via.placeholder.com/300x300?text=Backpack',
        isFeatured: true,
        reviews: 28
      },
      {
        id: '4',
        productName: 'Wireless Mouse',
        price: '29.99',
        originalPrice: '39.99',
        discount: 25,
        imageUrl: 'https://via.placeholder.com/300x300?text=Mouse',
        isFeatured: true,
        reviews: 56
      }
    ];
  }

  // Fallback recent products
  private getFallbackRecentProducts(): any[] {
    return [
      { id: '5', productName: 'USB Cable', price: '9.99', imageUrl: 'https://via.placeholder.com/300x300?text=USB+Cable' },
      { id: '6', productName: 'Phone Case', price: '14.99', imageUrl: 'https://via.placeholder.com/300x300?text=Phone+Case' },
      { id: '7', productName: 'Screen Protector', price: '7.99', imageUrl: 'https://via.placeholder.com/300x300?text=Screen+Protector' },
      { id: '8', productName: 'Power Bank', price: '39.99', imageUrl: 'https://via.placeholder.com/300x300?text=Power+Bank' }
    ];
  }

  // View all categories
  viewAllCategories(): void {
    this.router.navigate(['/products']);
  }

  // View all products
  viewAllProducts(): void {
    this.router.navigate(['/products'], { queryParams: { featured: true } });
  }

  // Clear recent products
  clearRecent(): void {
    this.recentProducts = [];
    localStorage.removeItem('recentProducts');
  }

  logout(): void {
    this.authService.logout();
    this.router.navigate(['/login']);
  }

  navigateTo(route: string): void {
    // Handle different routes
    const routes: { [key: string]: string } = {
      'cart': '/dashboard/customer/cart',
      'orders': '/dashboard/customer/orders',
      'wishlist': '/dashboard/customer/wishlist',
      'profile': '/dashboard/profile',
      'addresses': '/dashboard/customer/addresses'
    };

    const targetRoute = routes[route] || `/dashboard/customer/${route}`;
    this.router.navigate([targetRoute]);
  }

  viewProduct(productId: string): void {
    // Add to recent products when viewed
    const product = [...this.featuredProducts, ...this.recentProducts].find(p => p.id === productId);
    if (product) {
      // Remove if already exists
      this.recentProducts = this.recentProducts.filter(p => p.id !== productId);
      // Add to beginning
      this.recentProducts = [product, ...this.recentProducts].slice(0, 8);
      // Save to localStorage
      this.saveToLocalStorage('recentProducts', this.recentProducts);
    }

    this.router.navigate(['/product', productId]);
  }

  viewCategory(categoryId: string): void {
    this.router.navigate(['/products'], { queryParams: { category: categoryId } });
  }

  addToCart(product: any): void {
    // Check if product already in cart
    const existingItem = this.cartItems.find(item => item.id === product.id);

    if (existingItem) {
      existingItem.quantity = (existingItem.quantity || 1) + 1;
    } else {
      this.cartItems.push({ ...product, quantity: 1 });
    }

    // Update cart count
    this.cartCount = this.cartItems.reduce((total, item) => total + (item.quantity || 1), 0);

    // Save to localStorage
    this.saveToLocalStorage('cartItems', this.cartItems);

    // Show success message (could be replaced with a toast notification)
    alert(`${product.productName} added to cart!`);
  }

  addToWishlist(product: any): void {
    // Check if already in wishlist
    const exists = this.wishlistItems.some(item => item.id === product.id);

    if (!exists) {
      this.wishlistItems.push(product);
      this.wishlistCount = this.wishlistItems.length;

      // Save to localStorage
      this.saveToLocalStorage('wishlistItems', this.wishlistItems);

      // Show success message
      alert(`${product.productName} added to wishlist!`);
    } else {
      alert(`${product.productName} is already in your wishlist!`);
    }
  }

  removeFromWishlist(productId: string): void {
    this.wishlistItems = this.wishlistItems.filter(item => item.id !== productId);
    this.wishlistCount = this.wishlistItems.length;
    this.saveToLocalStorage('wishlistItems', this.wishlistItems);
  }

  updateCartQuantity(productId: string, quantity: number): void {
    const item = this.cartItems.find(item => item.id === productId);
    if (item) {
      item.quantity = quantity;
      if (quantity <= 0) {
        this.cartItems = this.cartItems.filter(item => item.id !== productId);
      }
      this.cartCount = this.cartItems.reduce((total, item) => total + (item.quantity || 1), 0);
      this.saveToLocalStorage('cartItems', this.cartItems);
    }
  }

  getCartTotal(): number {
    return this.cartItems.reduce((total, item) => {
      return total + (parseFloat(item.price) * (item.quantity || 1));
    }, 0);
  }

  clearCart(): void {
    this.cartItems = [];
    this.cartCount = 0;
    localStorage.removeItem('cartItems');
  }

  // Check if product is in wishlist
  isInWishlist(productId: string): boolean {
    return this.wishlistItems.some(item => item.id === productId);
  }

  // Check if product is in cart
  isInCart(productId: string): boolean {
    return this.cartItems.some(item => item.id === productId);
  }

  // Get cart item quantity
  getCartQuantity(productId: string): number {
    const item = this.cartItems.find(item => item.id === productId);
    return item ? item.quantity || 1 : 0;
  }
}
