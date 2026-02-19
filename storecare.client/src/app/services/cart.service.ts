import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable, BehaviorSubject, of } from 'rxjs';
import { catchError, map, tap } from 'rxjs/operators';
import { environment } from '../../environments/environment';

@Injectable({
    providedIn: 'root'
})
export class CartService {
    private apiUrl = `${environment.apiUrl}/cart`;
    private cartCountSubject = new BehaviorSubject<number>(0);
    cartCount$ = this.cartCountSubject.asObservable();

    constructor(private http: HttpClient) { }

    getCartCount(): Observable<number> {
        // Mock implementation for now, or fetch from API if available
        return this.http.get<{ count: number }>(`${this.apiUrl}/count`).pipe(
            map(res => res.count),
            tap(count => this.cartCountSubject.next(count)),
            catchError(() => of(0))
        );
    }

    addToCart(productId: string, quantity: number): Observable<boolean> {
        return this.http.post(`${this.apiUrl}/add`, { productId, quantity }).pipe(
            map(() => true),
            tap(() => {
                // Increment count locally for immediate feedback
                const currentCount = this.cartCountSubject.value;
                this.cartCountSubject.next(currentCount + quantity);
            }),
            catchError(error => {
                console.error('Error adding to cart', error);
                return of(false);
            })
        );
    }

    getCartItems(): Observable<any[]> {
        return this.http.get<any[]>(`${this.apiUrl}/items`).pipe(
            catchError(() => of([]))
        );
    }
}
