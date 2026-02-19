import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable, map } from 'rxjs';
import { environment } from '../../../environments/environment';

export interface ProductCategory {
    id: string;
    categoryCode: string;
    categoryName: string;
    categoryDescription: string;
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
    isActive: boolean;
    active?: boolean;
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
    isActive: boolean;
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
    isActive: boolean;
}

@Injectable({
    providedIn: 'root'
})
export class ProductService {
    private apiUrl = `${environment.apiUrl}/product`;
    private categoryApiUrl = `${environment.apiUrl}/productcategory`;
    private assignmentApiUrl = `${environment.apiUrl}/storeproductassignment`;

    constructor(private http: HttpClient) { }

    // ==================== HELPERS ====================
    private toFormData(data: any): FormData {
        const formData = new FormData();
        for (const key in data) {
            if (data.hasOwnProperty(key)) {
                const value = data[key];
                if (value instanceof File) {
                    formData.append(key, value);
                } else if (value !== null && value !== undefined) {
                    // Handle dates specially if needed, otherwise verify string conversion
                    formData.append(key, String(value));
                }
            }
        }
        return formData;
    }

    // ==================== CATEGORIES ====================
    getCategories(): Observable<ProductCategory[]> {
        return this.http.get<{ data: ProductCategory[] }>(this.categoryApiUrl).pipe(
            map(response => {
                const categories = response.data || [];
                return categories.map((cat: ProductCategory) => ({
                    ...cat,
                    imageUrl: this.getAbsoluteImageUrl(cat.categoryImageUrl || cat.categoryImage) || ''
                }));
            })
        );
    }

    private getAbsoluteImageUrl(url: string | undefined): string | undefined {
        if (!url) return undefined;
        if (url.startsWith('http')) return url;
        const cleanUrl = url.startsWith('/') ? url.substring(1) : url;
        const baseUrl = environment.apiUrl.replace('/api', '');
        return `${baseUrl}/${cleanUrl}`;
    }

    getCategoryById(id: string): Observable<ProductCategory> {
        return this.http.get<{ data: ProductCategory }>(`${this.categoryApiUrl}/${id}`).pipe(
            map(res => {
                const cat = res.data;
                return {
                    ...cat,
                    imageUrl: this.getAbsoluteImageUrl(cat.categoryImageUrl || cat.categoryImage) || ''
                };
            })
        );
    }

    createCategory(categoryData: FormData): Observable<ProductCategory> {
        return this.http.post<{ data: ProductCategory }>(this.categoryApiUrl, categoryData).pipe(map(res => res.data));
    }

    updateCategory(id: string, categoryData: FormData): Observable<ProductCategory> {
        return this.http.put<{ data: ProductCategory }>(`${this.categoryApiUrl}/${id}`, categoryData).pipe(map(res => res.data));
    }

    deleteCategory(id: string): Observable<void> {
        return this.http.delete<void>(`${this.categoryApiUrl}/${id}`);
    }

    // ==================== PRODUCTS ====================
    getProducts(page?: number, limit?: number): Observable<Product[]> {
        let url = this.apiUrl;
        if (page && limit) {
            url += `?page=${page}&limit=${limit}`;
        }

        return this.http.get<{ data: Product[] }>(url).pipe(
            map(response => {
                const products = response.data || [];
                return products.map((prod: any) => ({
                    ...prod,
                    imageUrl: this.getAbsoluteImageUrl(prod.productImageUrl || prod.productImage) || '',
                    thumbnailUrl: this.getAbsoluteImageUrl(prod.productImageUrl || prod.productImage) || ''
                }));
            })
        );
    }

    getStatuses(): Observable<any[]> {
        return this.http.get<any[]>(`${this.apiUrl}/statuses`);
    }

    getProductById(id: string): Observable<Product> {
        return this.http.get<{ data: Product }>(`${this.apiUrl}/${id}`).pipe(map(res => {
            const prod = res.data;
            return {
                ...prod,
                imageUrl: this.getAbsoluteImageUrl(prod.productImageUrl || prod.productImage) || ''
            };
        }));
    }

    getProductsByCategory(categoryId: string): Observable<Product[]> {
        return this.http.get<{ data: Product[] }>(`${this.apiUrl}/category/${categoryId}`).pipe(
            map(res => {
                const products = res.data || [];
                return products.map((p: any) => ({
                    ...p,
                    imageUrl: this.getAbsoluteImageUrl(p.productImageUrl || p.productImage) || ''
                }));
            })
        );
    }

    createProduct(productData: FormData | any): Observable<Product> {
        const data = productData instanceof FormData ? productData : this.toFormData(productData);
        return this.http.post<{ data: Product }>(this.apiUrl, data).pipe(map(res => res.data));
    }

    updateProduct(id: string, productData: FormData | any): Observable<Product> {
        const data = productData instanceof FormData ? productData : this.toFormData(productData);
        return this.http.put<{ data: Product }>(`${this.apiUrl}/${id}`, data).pipe(map(res => res.data));
    }

    deleteProduct(id: string): Observable<void> {
        return this.http.delete<void>(`${this.apiUrl}/${id}`);
    }

    // ==================== STORE PRODUCT ASSIGNMENTS ====================
    getAssignments(): Observable<StoreProductAssignment[]> {
        return this.http.get<{ data: StoreProductAssignment[] }>(this.assignmentApiUrl).pipe(
            map(response => {
                return response.data || [];
            })
        );
    }

    deleteAssignment(id: string): Observable<void> {
        return this.http.delete<void>(`${this.assignmentApiUrl}/${id}`);
    }

    getAssignedProducts(): Observable<StoreProductAssignment[]> {
        return this.http.get<{ data: StoreProductAssignment[] }>(`${this.assignmentApiUrl}/my-store`).pipe(map(res => res.data || []));
    }

    getAssignmentsByStore(storeId: string): Observable<StoreProductAssignment[]> {
        return this.http.get<{ data: StoreProductAssignment[] }>(`${this.assignmentApiUrl}/store/${storeId}`).pipe(map(res => res.data || []));
    }

    getAssignmentsByProduct(productId: string): Observable<StoreProductAssignment[]> {
        return this.http.get<{ data: StoreProductAssignment[] }>(`${this.assignmentApiUrl}/product/${productId}`).pipe(map(res => res.data || []));
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
