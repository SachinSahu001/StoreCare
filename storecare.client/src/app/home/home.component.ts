import { Component, OnInit } from '@angular/core';
import { Router } from '@angular/router';
import { AuthService } from '../services/auth.service';

@Component({
  selector: 'app-home',
  standalone: false,
  templateUrl: './home.component.html',
  styleUrls: ['./home.component.css'] // Fixed: changed styleUrl to styleUrls
})
export class HomeComponent implements OnInit {
  isLoggedIn = false;
  userRole: string | null = null;

  categories = [
    { id: 1, name: 'Electronics', icon: 'fas fa-laptop', items: 245, slug: 'electronics' },
    { id: 2, name: 'Fashion', icon: 'fas fa-tshirt', items: 567, slug: 'fashion' },
    { id: 3, name: 'Home & Living', icon: 'fas fa-home', items: 189, slug: 'home-living' },
    { id: 4, name: 'Sports', icon: 'fas fa-futbol', items: 123, slug: 'sports' },
    { id: 5, name: 'Books', icon: 'fas fa-book', items: 432, slug: 'books' },
    { id: 6, name: 'Toys', icon: 'fas fa-gamepad', items: 156, slug: 'toys' },
    { id: 7, name: 'Beauty', icon: 'fas fa-spa', items: 89, slug: 'beauty' },
    { id: 8, name: 'Jewelry', icon: 'fas fa-gem', items: 67, slug: 'jewelry' }
  ];

  featuredProducts = [
    {
      id: 1,
      name: 'Wireless Headphones',
      price: 99.99,
      originalPrice: 149.99,
      discount: 33,
      image: 'assets/images/products/headphones.jpg',
      hoverImage: 'assets/images/products/headphones-2.jpg',
      reviews: 45,
      rating: 4.5,
      isNew: true,
      isFeatured: true
    },
    {
      id: 2,
      name: 'Smart Watch Pro',
      price: 199.99,
      originalPrice: 299.99,
      discount: 33,
      image: 'assets/images/products/smartwatch.jpg',
      hoverImage: 'assets/images/products/smartwatch-2.jpg',
      reviews: 32,
      rating: 4,
      isNew: true,
      isFeatured: true
    },
    {
      id: 3,
      name: 'Laptop Backpack',
      price: 49.99,
      originalPrice: 79.99,
      discount: 38,
      image: 'assets/images/products/backpack.jpg',
      hoverImage: 'assets/images/products/backpack-2.jpg',
      reviews: 28,
      rating: 5,
      isNew: false,
      isFeatured: true
    },
    {
      id: 4,
      name: 'Running Shoes',
      price: 79.99,
      originalPrice: 129.99,
      discount: 38,
      image: 'assets/images/products/shoes.jpg',
      hoverImage: 'assets/images/products/shoes-2.jpg',
      reviews: 56,
      rating: 4.5,
      isNew: false,
      isFeatured: true
    },
    {
      id: 5,
      name: 'Smart Phone',
      price: 699.99,
      originalPrice: 899.99,
      discount: 22,
      image: 'assets/images/products/phone.jpg',
      hoverImage: 'assets/images/products/phone-2.jpg',
      reviews: 124,
      rating: 4.8,
      isNew: true,
      isFeatured: false
    },
    {
      id: 6,
      name: 'Coffee Maker',
      price: 89.99,
      originalPrice: 129.99,
      discount: 31,
      image: 'assets/images/products/coffee-maker.jpg',
      hoverImage: 'assets/images/products/coffee-maker-2.jpg',
      reviews: 67,
      rating: 4.3,
      isNew: false,
      isFeatured: false
    }
  ];

  testimonials = [
    {
      id: 1,
      name: 'John Doe',
      role: 'Customer',
      comment: 'Amazing products and excellent customer service! Will definitely shop again.',
      rating: 5,
      image: 'assets/images/users/user1.jpg'
    },
    {
      id: 2,
      name: 'Jane Smith',
      role: 'Store Owner',
      comment: 'StoreCare has been a game-changer for my business. Highly recommended!',
      rating: 5,
      image: 'assets/images/users/user2.jpg'
    },
    {
      id: 3,
      name: 'Mike Johnson',
      role: 'Customer',
      comment: 'Fast shipping and great quality products. Best online store ever!',
      rating: 5,
      image: 'assets/images/users/user3.jpg'
    }
  ];

  brands = [
    { name: 'Nike', logo: 'assets/images/brands/nike.png' },
    { name: 'Apple', logo: 'assets/images/brands/apple.png' },
    { name: 'Samsung', logo: 'assets/images/brands/samsung.png' },
    { name: 'Adidas', logo: 'assets/images/brands/adidas.png' },
    { name: 'Sony', logo: 'assets/images/brands/sony.png' },
    { name: 'LG', logo: 'assets/images/brands/lg.png' }
  ];

  constructor(
    private router: Router,
    private authService: AuthService
  ) { }

  ngOnInit(): void {
    this.isLoggedIn = this.authService.isLoggedIn();
    this.userRole = this.authService.getRole();
  }

  // Navigation methods
  navigateToCategory(categorySlug: string): void {
    this.router.navigate(['/products', categorySlug]);
  }

  navigateToProduct(productId: number): void {
    this.router.navigate(['/product', productId]);
  }

  navigateToShop(): void {
    this.router.navigate(['/products']);
  }

  navigateToAbout(): void {
    this.router.navigate(['/about']);
  }

  addToCart(product: any): void {
    if (!this.isLoggedIn) {
      this.router.navigate(['/login'], {
        queryParams: { returnUrl: this.router.url }
      });
      return;
    }
    console.log('Adding to cart:', product);
    // Implement add to cart logic
  }

  quickView(product: any): void {
    console.log('Quick view:', product);
    // Implement quick view modal
  }

  getStarArray(rating: number): number[] {
    return Array(Math.floor(rating)).fill(0);
  }

  hasHalfStar(rating: number): boolean {
    return rating % 1 !== 0;
  }

  getEmptyStarArray(rating: number): number[] {
    return Array(5 - Math.ceil(rating)).fill(0);
  }
}
