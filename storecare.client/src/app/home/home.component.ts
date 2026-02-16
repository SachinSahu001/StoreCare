import { Component } from '@angular/core';

@Component({
  selector: 'app-home',
  standalone: false,
  templateUrl: './home.component.html',
  styleUrl: './home.component.css'
})
export class HomeComponent {
  categories = [
    { name: 'Electronics', icon: 'fas fa-laptop', items: 245 },
    { name: 'Fashion', icon: 'fas fa-tshirt', items: 567 },
    { name: 'Home & Living', icon: 'fas fa-home', items: 189 },
    { name: 'Sports', icon: 'fas fa-futbol', items: 123 },
    { name: 'Books', icon: 'fas fa-book', items: 432 },
    { name: 'Toys', icon: 'fas fa-gamepad', items: 156 }
  ];

  featuredProducts = [
    {
      name: 'Wireless Headphones',
      price: 99.99,
      image: 'https://via.placeholder.com/300x200',
      reviews: 45,
      rating: 4.5
    },
    {
      name: 'Smart Watch',
      price: 199.99,
      image: 'https://via.placeholder.com/300x200',
      reviews: 32,
      rating: 4
    },
    {
      name: 'Laptop Backpack',
      price: 49.99,
      image: 'https://via.placeholder.com/300x200',
      reviews: 28,
      rating: 5
    },
    {
      name: 'Running Shoes',
      price: 79.99,
      image: 'https://via.placeholder.com/300x200',
      reviews: 56,
      rating: 4.5
    }
  ];
}
