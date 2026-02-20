import { Injectable } from '@angular/core';
import { HttpClient, HttpErrorResponse, HttpHeaders } from '@angular/common/http';
import { BehaviorSubject, Observable, tap, catchError, throwError, timeout, map } from 'rxjs';
import { Router } from '@angular/router';
import { environment } from '../../environments/environment';

export interface RegisterRequest {
  fullName: string;
  email: string;
  phone: string;
  password: string;
  confirmPassword: string;
  role?: string;
  storeId?: string;
}

export interface LoginRequest {
  email: string;
  password: string;
}

export interface AuthResponse {
  token: string;
  role: string;
  fullName: string;
  email: string;
  storeId?: string;
  profilePicture?: string;
  message: string;
}

export interface UserProfile {
  id: string;
  userCode: string;
  fullName: string;
  email: string;
  phone: string;
  role: string;
  status: string;
  statusColor?: string;    // e.g. 'green' | 'red' | 'orange' — for badge CSS
  active: boolean;
  isActive?: boolean;      // soft-delete state (mirrors active)
  storeId?: string;
  storeName?: string;
  profilePicture?: string;
  profilePictureUrl?: string;
  createdBy: string;
  createdDate: Date;
  modifiedBy: string;
  modifiedDate: Date;
  lastLogin?: Date;
}

@Injectable({
  providedIn: 'root'
})
export class AuthService {
  private apiUrl = `${environment.apiUrl}/auth`;
  private tokenKey = 'auth_token';
  private userKey = 'current_user';

  private isAuthenticatedSubject = new BehaviorSubject<boolean>(this.hasToken());
  isAuthenticated$ = this.isAuthenticatedSubject.asObservable();

  private currentUserSubject = new BehaviorSubject<UserProfile | null>(this.getStoredUser());
  currentUser$ = this.currentUserSubject.asObservable();

  constructor(
    private http: HttpClient,
    private router: Router
  ) { }

  // ==================== REGISTRATION ====================
  register(data: RegisterRequest): Observable<any> {
    console.log('Registering user:', data.email);
    return this.http.post(`${this.apiUrl}/register`, data, {
      headers: {
        'Content-Type': 'application/json',
        'Accept': 'application/json'
      }
    }).pipe(
      timeout(30000), // 30 seconds timeout
      tap(response => console.log('Registration response:', response)),
      catchError(this.handleError)
    );
  }

  // ==================== USER MANAGEMENT ====================
  getUsers(role?: string): Observable<UserProfile[]> {
    let url = `${this.apiUrl}/users`;
    if (role) {
      url += `?role=${role}`;
    }
    return this.http.get<any>(url).pipe(
      map(response => {
        const users = Array.isArray(response) ? response : (response.data || []);
        return users.map((user: any) => ({
          ...user,
          // Backend sends 'profilePicture' (raw path); normalise to absolute URL
          profilePictureUrl: this.getAbsoluteImageUrl(user.profilePictureUrl || user.profilePicture)
        }));
      }),
      catchError(this.handleError)
    );
  }

  private getAbsoluteImageUrl(url: string | undefined): string | undefined {
    if (!url) return undefined;
    if (url.startsWith('http')) return url;
    return `${environment.apiUrl.replace('/api', '')}/${url.startsWith('/') ? url.substring(1) : url}`;
  }

  toggleUserStatus(userId: string, active: boolean): Observable<any> {
    return this.http.patch(
      `${this.apiUrl}/users/${userId}/toggle-status`,
      active,   // raw boolean — backend reads body as bool, not an object
      { headers: new HttpHeaders({ 'Content-Type': 'application/json' }) }
    ).pipe(
      catchError(this.handleError)
    );
  }

  createStoreAdmin(data: RegisterRequest): Observable<any> {
    return this.http.post(`${this.apiUrl}/storeadmin`, data).pipe(
      catchError(this.handleError)
    );
  }

  // ==================== LOGIN ====================
  login(data: LoginRequest): Observable<AuthResponse> {
    console.log('Logging in user:', data.email);
    return this.http.post<AuthResponse>(`${this.apiUrl}/login`, data, {
      headers: {
        'Content-Type': 'application/json',
        'Accept': 'application/json'
      }
    }).pipe(
      timeout(30000), // 30 seconds timeout
      tap(response => {
        console.log('Login response:', response);
        this.setToken(response.token);
        const user: Partial<UserProfile> = {
          fullName: response.fullName,
          email: response.email,
          role: response.role,
          storeId: response.storeId,
          profilePictureUrl: this.getAbsoluteImageUrl(response.profilePicture)
        };
        this.setUser(user);
        this.isAuthenticatedSubject.next(true);
      }),
      catchError(this.handleError)
    );
  }

  // ==================== LOGOUT ====================
  logout(): void {
    this.http.post(`${this.apiUrl}/logout`, {}).subscribe({
      next: () => {
        this.clearStorage();
        this.isAuthenticatedSubject.next(false);
        this.router.navigate(['/login']);
      },
      error: () => {
        this.clearStorage();
        this.isAuthenticatedSubject.next(false);
        this.router.navigate(['/login']);
      }
    });
  }

  // ==================== GET PROFILE ====================
  getProfile(): Observable<UserProfile> {
    return this.http.get<UserProfile>(`${this.apiUrl}/profile`).pipe(
      map(user => ({
        ...user,
        profilePictureUrl: this.getAbsoluteImageUrl(user.profilePictureUrl)
      })),
      tap(user => {
        this.setUser(user);
        this.currentUserSubject.next(user);
      }),
      catchError(this.handleError)
    );
  }

  // ==================== WHO AM I ====================
  whoAmI(): Observable<any> {
    return this.http.get(`${this.apiUrl}/whoami`).pipe(
      catchError(this.handleError)
    );
  }

  // ==================== TOKEN MANAGEMENT ====================
  setToken(token: string): void {
    localStorage.setItem(this.tokenKey, token);
  }

  getToken(): string | null {
    return localStorage.getItem(this.tokenKey);
  }

  removeToken(): void {
    localStorage.removeItem(this.tokenKey);
  }

  // ==================== PROFILE MANAGEMENT ====================
  updateProfile(data: Partial<UserProfile>): Observable<UserProfile> {
    return this.http.put<UserProfile>(`${this.apiUrl}/profile`, data).pipe(
      map(user => ({
        ...user,
        profilePictureUrl: this.getAbsoluteImageUrl(user.profilePictureUrl)
      })),
      tap(user => {
        this.setUser(user); // Update local storage
      }),
      catchError(this.handleError)
    );
  }

  changePassword(data: any): Observable<any> {
    return this.http.put(`${this.apiUrl}/change-password`, data).pipe(
      catchError(this.handleError)
    );
  }

  uploadProfilePicture(data: FormData): Observable<any> {
    return this.http.post(`${this.apiUrl}/profile-picture`, data).pipe(
      map((res: any) => ({
        ...res,
        profilePictureUrl: this.getAbsoluteImageUrl(res.profilePictureUrl)
      })),
      catchError(this.handleError)
    );
  }

  getMyLoginHistory(): Observable<any[]> {
    return this.http.get<any[]>(`${this.apiUrl}/my-login-history`).pipe(
      catchError(this.handleError)
    );
  }

  hasToken(): boolean {
    return !!this.getToken();
  }

  // ==================== USER MANAGEMENT ====================
  setUser(user: Partial<UserProfile>): void {
    const currentUser = this.getStoredUser() || {};
    const updatedUser = { ...currentUser, ...user };
    localStorage.setItem(this.userKey, JSON.stringify(updatedUser));
    this.currentUserSubject.next(updatedUser as UserProfile);
  }

  getStoredUser(): UserProfile | null {
    const userStr = localStorage.getItem(this.userKey);
    if (userStr) {
      try {
        return JSON.parse(userStr);
      } catch {
        return null;
      }
    }
    return null;
  }

  removeUser(): void {
    localStorage.removeItem(this.userKey);
    this.currentUserSubject.next(null);
  }

  clearStorage(): void {
    this.removeToken();
    this.removeUser();
  }

  // ==================== HELPER METHODS ====================
  isLoggedIn(): boolean {
    return this.hasToken();
  }

  getCurrentUser(): UserProfile | null {
    return this.currentUserSubject.value;
  }

  getRole(): string | null {
    return this.currentUserSubject.value?.role || null;
  }

  getFullName(): string | null {
    return this.currentUserSubject.value?.fullName || null;
  }

  // ==================== ROLE-BASED REDIRECTION ====================
  redirectBasedOnRole(role: string): void {
    console.log('Redirecting based on role:', role);
    switch (role) {
      case 'SuperAdmin':
        this.router.navigate(['/dashboard/superadmin']);
        break;
      case 'StoreAdmin':
        this.router.navigate(['/dashboard/storeadmin']);
        break;
      case 'Customer':
        this.router.navigate(['/dashboard/customer']);
        break;
      default:
        this.router.navigate(['/']);
        break;
    }
  }

  // ==================== ERROR HANDLER ====================
  private handleError(error: HttpErrorResponse) {
    console.error('API Error Details:', {
      status: error.status,
      statusText: error.statusText,
      message: error.message,
      error: error.error,
      url: error.url,
      name: error.name,
      ok: error.ok
    });

    let errorMessage = 'An error occurred';

    if (error.error instanceof ErrorEvent) {
      // Client-side error
      errorMessage = `Client Error: ${error.error.message}`;
      console.error('Client-side error:', error.error.message);
    } else {
      // Server-side error
      if (error.status === 0) {
        // This usually means CORS or network issue
        errorMessage = 'Cannot connect to server. Please check:\n' +
          '1. Server is running on https://localhost:7066\n' +
          '2. CORS is configured correctly\n' +
          '3. You have accepted the SSL certificate';
        console.error('Server connection failed - CORS or server not running');
        console.error('Attempted URL:', error.url);
      } else if (error.status === 401) {
        errorMessage = 'Invalid email or password';
      } else if (error.status === 400) {
        errorMessage = error.error?.message || 'Bad request';
      } else if (error.status === 404) {
        errorMessage = `API endpoint not found: ${error.url}`;
      } else if (error.status === 500) {
        errorMessage = 'Server error. Please try again later.';
      } else {
        errorMessage = error.error?.message || `Server Error: ${error.status} - ${error.statusText}`;
      }
    }

    return throwError(() => new Error(errorMessage));
  }
}
