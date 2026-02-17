import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../environments/environment';

export interface Store {
  id: string;
  storeCode: string;
  storeName: string;
  storeEmail: string;
  storePhone: string;
  storeAddress: string;
  city: string;
  state: string;
  pincode: string;
  gstNumber: string;
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
export class StoreService {
  private apiUrl = `${environment.apiUrl}/store`;

  constructor(private http: HttpClient) { }

  getStores(): Observable<Store[]> {
    return this.http.get<Store[]>(this.apiUrl);
  }

  getStoreById(id: string): Observable<Store> {
    return this.http.get<Store>(`${this.apiUrl}/${id}`);
  }

  getOwnStore(): Observable<Store> {
    return this.http.get<Store>(`${this.apiUrl}/my-store`);
  }

  createStore(store: Partial<Store>): Observable<Store> {
    return this.http.post<Store>(this.apiUrl, store);
  }

  updateStore(id: string, store: Partial<Store>): Observable<Store> {
    return this.http.put<Store>(`${this.apiUrl}/${id}`, store);
  }

  deleteStore(id: string): Observable<void> {
    return this.http.delete<void>(`${this.apiUrl}/${id}`);
  }

  toggleStoreStatus(id: string, active: boolean): Observable<any> {
    return this.http.patch(`${this.apiUrl}/${id}/toggle-status`, { active });
  }
}
