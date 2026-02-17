import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../environments/environment';

export interface ProductCategory {
  id: string;
  categoryCode: string;
  categoryName: string;
  description: string;
  imageUrl: string;
  displayOrder: number;
  statusId: number;
  status?: string;
  createdBy: string;
  createdDate: Date;
  modifiedBy: string;
  modifiedDate: Date;
  active: boolean;
}

export interface Product {
  id: string;
  productCode: string;
  productName: string;
  description: string;
  price: number;
  mrp: number;
  gstRate: number;
  categoryId: string;
  categoryName?: string;
  brand: string;
  model: string;
  imageUrl: string;
  thumbnailUrl: string;
  stockQuantity: number;
  unit: string;
  hsnCode: string;
  statusId: number;
  status?: string;
  createdBy: string;
  createdDate: Date;
  modifiedBy: string;
  modifiedDate: Date;
  active: boolean;
}

export interface StoreProductAssignment {
  id: string;
  storeId: string;
  storeName?: string;
  productId: string;
  productName?: string;
  sellingPrice: number;
  stockQuantity: number;
  minStockLevel: number;
  maxStockLevel: number;
  reorderLevel: number;
  shelfLocation: string;
  statusId: number;
  status?: string;
  createdBy: string;
  createdDate: Date;
  modifiedBy: string;
  modifiedDate: Date;
  active: boolean;
}

@Injectable({
  providedIn: 'root'
})
export class ProductService {
  private apiUrl = `${environment.apiUrl}/product`;
  private categoryApiUrl = `${environment.apiUrl}/productcategory`;
  private assignmentApiUrl = `${environment.apiUrl}/storeproductassignment`;

  constructor(private http: HttpClient) { }

  // ==================== CATEGORIES ====================
  getCategories(): Observable<ProductCategory[]> {
    return this.http.get<ProductCategory[]>(this.categoryApiUrl);
  }

  getCategoryById(id: string): Observable<ProductCategory> {
    return this.http.get<ProductCategory>(`${this.categoryApiUrl}/${id}`);
  }

  createCategory(category: Partial<ProductCategory>): Observable<ProductCategory> {
    return this.http.post<ProductCategory>(this.categoryApiUrl, category);
  }

  updateCategory(id: string, category: Partial<ProductCategory>): Observable<ProductCategory> {
    return this.http.put<ProductCategory>(`${this.categoryApiUrl}/${id}`, category);
  }

  deleteCategory(id: string): Observable<void> {
    return this.http.delete<void>(`${this.categoryApiUrl}/${id}`);
  }

  // ==================== PRODUCTS ====================
  getProducts(): Observable<Product[]> {
    return this.http.get<Product[]>(this.apiUrl);
  }

  getProductById(id: string): Observable<Product> {
    return this.http.get<Product>(`${this.apiUrl}/${id}`);
  }

  getProductsByCategory(categoryId: string): Observable<Product[]> {
    return this.http.get<Product[]>(`${this.apiUrl}/category/${categoryId}`);
  }

  createProduct(product: Partial<Product>): Observable<Product> {
    return this.http.post<Product>(this.apiUrl, product);
  }

  updateProduct(id: string, product: Partial<Product>): Observable<Product> {
    return this.http.put<Product>(`${this.apiUrl}/${id}`, product);
  }

  deleteProduct(id: string): Observable<void> {
    return this.http.delete<void>(`${this.apiUrl}/${id}`);
  }

  // ==================== STORE PRODUCT ASSIGNMENTS ====================
  getAssignments(): Observable<StoreProductAssignment[]> {
    return this.http.get<StoreProductAssignment[]>(this.assignmentApiUrl);
  }

  getAssignedProducts(): Observable<StoreProductAssignment[]> {
    return this.http.get<StoreProductAssignment[]>(`${this.assignmentApiUrl}/my-store`);
  }

  getAssignmentsByStore(storeId: string): Observable<StoreProductAssignment[]> {
    return this.http.get<StoreProductAssignment[]>(`${this.assignmentApiUrl}/store/${storeId}`);
  }

  getAssignmentsByProduct(productId: string): Observable<StoreProductAssignment[]> {
    return this.http.get<StoreProductAssignment[]>(`${this.assignmentApiUrl}/product/${productId}`);
  }

  createAssignment(assignment: Partial<StoreProductAssignment>): Observable<StoreProductAssignment> {
    return this.http.post<StoreProductAssignment>(this.assignmentApiUrl, assignment);
  }

  updateAssignment(id: string, assignment: Partial<StoreProductAssignment>): Observable<StoreProductAssignment> {
    return this.http.put<StoreProductAssignment>(`${this.assignmentApiUrl}/${id}`, assignment);
  }

  deleteAssignment(id: string): Observable<void> {
    return this.http.delete<void>(`${this.assignmentApiUrl}/${id}`);
  }
}
