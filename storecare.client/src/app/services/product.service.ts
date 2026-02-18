import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable, map } from 'rxjs';
import { environment } from '../../environments/environment';

export interface ProductCategory {
  id: string;
  categoryCode: string;
  categoryName: string;
  categoryDescription: string; // Changed from description to match backend
  categoryImage?: string;
  categoryImageUrl?: string;
  imageUrl: string;
  displayOrder: number;
  statusId: number;
  status?: string;
  createdBy: string;
  createdDate: Date;
  modifiedBy: string;
  modifiedDate: Date;
  isActive: boolean; // Changed from active to match DTO if needed
  active?: boolean; // Keep optional for backward compat if needed during transition, or remove if strict.
}

export interface Product {
  id: string;
  productCode: string;
  productName: string;
  productDescription: string; // Changed from description
  price: number;
  mrp: number;
  gstRate: number;
  categoryId: string;
  categoryName?: string;
  brand: string;
  model: string;
  productImage?: string;
  productImageUrl?: string;
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
  isFeatured?: boolean;
  isActive: boolean; // Changed from active
  active?: boolean;
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
  isActive: boolean; // Changed from active
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
    return this.http.get<any>(this.categoryApiUrl).pipe(
      map(response => {
        const categories = response.data || []; // Explicitly use .data based on API
        return categories.map((cat: ProductCategory) => ({
          ...cat,
          imageUrl: this.getAbsoluteImageUrl(cat.categoryImageUrl || cat.categoryImage) // Use explicit URL field if available, fallback to path
        }));
      })
    );
  }

  private getAbsoluteImageUrl(url: string | undefined): string | undefined {
    if (!url) return undefined;
    if (url.startsWith('http')) return url;
    // Remove leading slash if present to avoid double slashes if apiUrl ends with slash
    const cleanUrl = url.startsWith('/') ? url.substring(1) : url;
    const baseUrl = environment.apiUrl.replace('/api', '');
    return `${baseUrl}/${cleanUrl}`;
  }

  getCategoryById(id: string): Observable<ProductCategory> {
    return this.http.get<any>(`${this.categoryApiUrl}/${id}`).pipe(
      map(res => {
        const cat = res.data;
        return {
          ...cat,
          imageUrl: this.getAbsoluteImageUrl(cat.categoryImageUrl || cat.categoryImage)
        };
      })
    );
  }

  // Changed to accept FormData directly to ensure component handles file appending, 
  // BUT strict key names 'CategoryImage' etc are enforced by component. 
  // However, helpful to have a type-safe wrapper if possible, but FormData is standard.
  createCategory(categoryData: FormData): Observable<ProductCategory> {
    return this.http.post<any>(this.categoryApiUrl, categoryData).pipe(map(res => res.data));
  }

  updateCategory(id: string, categoryData: FormData): Observable<ProductCategory> {
    return this.http.put<any>(`${this.categoryApiUrl}/${id}`, categoryData).pipe(map(res => res.data));
  }

  deleteCategory(id: string): Observable<void> {
    return this.http.delete<void>(`${this.categoryApiUrl}/${id}`);
  }

  // ==================== PRODUCTS ====================
  getProducts(): Observable<Product[]> {
    return this.http.get<any>(this.apiUrl).pipe(
      map(response => {
        const products = response.data || [];
        return products.map((prod: any) => ({
          ...prod,
          imageUrl: this.getAbsoluteImageUrl(prod.productImageUrl || prod.productImage),
          thumbnailUrl: this.getAbsoluteImageUrl(prod.productImageUrl || prod.productImage) // Use same for now if no specific thumb
        }));
      })
    );
  }

  getProductById(id: string): Observable<Product> {
    return this.http.get<any>(`${this.apiUrl}/${id}`).pipe(map(res => {
      const prod = res.data;
      return {
        ...prod,
        imageUrl: this.getAbsoluteImageUrl(prod.productImageUrl || prod.productImage)
      };
    }));
  }

  getProductsByCategory(categoryId: string): Observable<Product[]> {
    return this.http.get<any>(`${this.apiUrl}/category/${categoryId}`).pipe(
      map(res => {
        const products = res.data || [];
        return products.map((p: any) => ({
          ...p,
          imageUrl: this.getAbsoluteImageUrl(p.productImageUrl || p.productImage)
        }));
      })
    );
  }

  createProduct(productData: FormData): Observable<Product> {
    return this.http.post<any>(this.apiUrl, productData).pipe(map(res => res.data));
  }

  updateProduct(id: string, productData: FormData): Observable<Product> {
    return this.http.put<any>(`${this.apiUrl}/${id}`, productData).pipe(map(res => res.data));
  }

  deleteProduct(id: string): Observable<void> {
    return this.http.delete<void>(`${this.apiUrl}/${id}`);
  }

  // ==================== STORE PRODUCT ASSIGNMENTS ====================
  getAssignments(): Observable<StoreProductAssignment[]> {
    return this.http.get<any>(this.assignmentApiUrl).pipe(
      map(response => {
        return response.data || [];
      })
    );
  }

  deleteAssignment(id: string): Observable<void> {
    return this.http.delete<void>(`${this.assignmentApiUrl}/${id}`);
  }

  getAssignedProducts(): Observable<StoreProductAssignment[]> {
    return this.http.get<any>(`${this.assignmentApiUrl}/my-store`).pipe(map(res => res.data || []));
  }

  getAssignmentsByStore(storeId: string): Observable<StoreProductAssignment[]> {
    return this.http.get<any>(`${this.assignmentApiUrl}/store/${storeId}`).pipe(map(res => res.data || []));
  }

  getAssignmentsByProduct(productId: string): Observable<StoreProductAssignment[]> {
    return this.http.get<any>(`${this.assignmentApiUrl}/product/${productId}`).pipe(map(res => res.data || []));
  }

  createAssignment(assignment: Partial<StoreProductAssignment>): Observable<StoreProductAssignment> {
    return this.http.post<StoreProductAssignment>(this.assignmentApiUrl, assignment);
  }

  updateAssignment(id: string, assignment: Partial<StoreProductAssignment>): Observable<StoreProductAssignment> {
    return this.http.put<StoreProductAssignment>(`${this.assignmentApiUrl}/${id}`, assignment);
  }

  assignProductsByCategory(data: any): Observable<any> {
    return this.http.post(`${this.assignmentApiUrl}/by-category`, data);
  }

  getProductsByCategoryForAssignment(categoryId: string, storeId?: string): Observable<any> {
    let url = `${this.assignmentApiUrl}/products-by-category/${categoryId}`;
    if (storeId) {
      url += `?storeId=${storeId}`;
    }
    return this.http.get<any>(url).pipe(
      map(response => {
        if (response && response.data && Array.isArray(response.data)) {
          response.data = response.data.map((p: any) => ({
            ...p,
            productImageUrl: this.getAbsoluteImageUrl(p.productImageUrl)
          }));
        }
        return response;
      })
    );
  }
}
