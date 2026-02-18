import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable, map } from 'rxjs';
import { environment } from '../../environments/environment';
import { UserProfile } from './auth.service';

export interface Store {
  id: string;
  storeCode: string;
  storeName: string;
  email: string;
  contactNumber: string;
  address: string;
  storeLogo?: string;
  storeLogoUrl?: string;
  statusId: number;
  status?: string;
  statusColor?: string;
  createdBy?: string;
  createdDate?: Date;
  modifiedBy?: string;
  modifiedDate?: Date;
  isActive: boolean;
  totalEmployees?: number;
  totalProducts?: number;
}

@Injectable({
  providedIn: 'root'
})
export class StoreService {
  private apiUrl = `${environment.apiUrl}/store`;

  constructor(private http: HttpClient) { }

  getStores(): Observable<Store[]> {
    return this.http.get<any>(this.apiUrl).pipe(
      map(response => {
        const stores = response.data || [];
        return stores.map((s: any) => ({
          ...s,
          storeLogoUrl: this.getAbsoluteImageUrl(s.storeLogoUrl || s.storeLogo)
        }));
      })
    );
  }

  getOwnStore(): Observable<Store | null> {
    // Since there is no specific /api/Store/my-store endpoint mentioned in the user prompt (it says "There is no getOwnStore endpoint"),
    // we will fetch all stores and filter by the current user's storeId if available.
    // Ideally, the backend should provide this, but we will filter client-side as requested.
    return this.getStores().pipe(
      map(stores => {
        const userStr = localStorage.getItem('current_user');
        if (!userStr) return null;
        try {
          const user = JSON.parse(userStr) as UserProfile;
          return stores.find(s => s.id === user.storeId) || null;
        } catch {
          return null;
        }
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

  getStoreById(id: string): Observable<Store> {
    return this.http.get<any>(`${this.apiUrl}/${id}`).pipe(
      map(res => {
        const s = res.data;
        return {
          ...s,
          storeLogoUrl: this.getAbsoluteImageUrl(s.storeLogoUrl || s.storeLogo)
        };
      })
    );
  }

  createStore(storeData: FormData): Observable<Store> {
    return this.http.post<any>(this.apiUrl, storeData).pipe(map(res => res.data));
  }

  updateStore(id: string, storeData: FormData): Observable<Store> {
    return this.http.put<any>(`${this.apiUrl}/${id}`, storeData).pipe(map(res => res.data));
  }

  deleteStore(id: string): Observable<void> {
    return this.http.delete<void>(`${this.apiUrl}/${id}`);
  }

  toggleStoreStatus(id: string, statusId: number): Observable<any> {
    return this.http.patch(`${this.apiUrl}/${id}/status`, { statusId });
  }
}
