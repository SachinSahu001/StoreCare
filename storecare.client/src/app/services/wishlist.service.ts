import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable, BehaviorSubject, of } from 'rxjs';
import { catchError, map, tap } from 'rxjs/operators';
import { environment } from '../../environments/environment';

@Injectable({
    providedIn: 'root'
})
export class WishlistService {
    private apiUrl = `${environment.apiUrl}/wishlist`;
    private wishlistCountSubject = new BehaviorSubject<number>(0);
    wishlistCount$ = this.wishlistCountSubject.asObservable();
    private wishlistItems: Set<string> = new Set();

    constructor(private http: HttpClient) { }

    getWishlistCount(): Observable<number> {
        return this.http.get<{ count: number }>(`${this.apiUrl}/count`).pipe(
            map(res => res.count),
            tap(count => this.wishlistCountSubject.next(count)),
            catchError(() => of(0))
        );
    }

    addToWishlist(productId: string): Observable<boolean> {
        return this.http.post(`${this.apiUrl}/add`, { productId }).pipe(
            map(() => true),
            tap(() => {
                this.wishlistItems.add(productId);
                const currentCount = this.wishlistCountSubject.value;
                this.wishlistCountSubject.next(currentCount + 1);
            }),
            catchError(error => {
                console.error('Error adding to wishlist', error);
                return of(false);
            })
        );
    }

    removeFromWishlist(productId: string): Observable<boolean> {
        return this.http.delete(`${this.apiUrl}/${productId}`).pipe(
            map(() => true),
            tap(() => {
                this.wishlistItems.delete(productId);
                const currentCount = this.wishlistCountSubject.value;
                if (currentCount > 0) this.wishlistCountSubject.next(currentCount - 1);
            }),
            catchError(error => {
                console.error('Error removing from wishlist', error);
                return of(false);
            })
        );
    }

    isInWishlist(productId: string): boolean {
        return this.wishlistItems.has(productId);
    }
}
