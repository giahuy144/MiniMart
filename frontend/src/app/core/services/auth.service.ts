import { Injectable, signal, computed } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Router } from '@angular/router';
import { Observable, tap } from 'rxjs';
import { environment } from '../../../environments/environment';
import { ApiResponse } from '../models/api-response.model';
import { AuthResponse, LoginRequest, RegisterRequest, UserInfo } from '../models/auth.model';

const TOKEN_KEY = 'minimart_token';
const USER_KEY  = 'minimart_user';

@Injectable({ providedIn: 'root' })
export class AuthService {
  private readonly baseUrl = `${environment.apiUrl}/api/auth`;

  // Reactive state bằng Angular Signals
  private _user = signal<AuthResponse | null>(this.loadUser());
  readonly currentUser = this._user.asReadonly();
  readonly isLoggedIn = computed(() => !!this._user());
  readonly userRole   = computed(() => this._user()?.role ?? '');
  readonly fullName   = computed(() => this._user()?.fullName ?? '');

  constructor(private http: HttpClient, private router: Router) {}

  // ─── Đăng nhập ───────────────────────────────────────────────
  login(payload: LoginRequest): Observable<ApiResponse<AuthResponse>> {
    return this.http
      .post<ApiResponse<AuthResponse>>(`${this.baseUrl}/login`, payload)
      .pipe(
        tap(res => {
          if (res.success && res.data) {
            this.saveSession(res.data);
          }
        })
      );
  }

  // ─── Đăng ký ─────────────────────────────────────────────────
  register(payload: RegisterRequest): Observable<ApiResponse<UserInfo>> {
    return this.http.post<ApiResponse<UserInfo>>(`${this.baseUrl}/register`, payload);
  }

  // ─── Đăng xuất ───────────────────────────────────────────────
  logout(): void {
    localStorage.removeItem(TOKEN_KEY);
    localStorage.removeItem(USER_KEY);
    this._user.set(null);
    this.router.navigate(['/login']);
  }

  // ─── Lấy JWT token (dùng cho Interceptor) ────────────────────
  getToken(): string | null {
    return localStorage.getItem(TOKEN_KEY);
  }

  // ─── Helpers ─────────────────────────────────────────────────
  private saveSession(user: AuthResponse): void {
    localStorage.setItem(TOKEN_KEY, user.token);
    localStorage.setItem(USER_KEY, JSON.stringify(user));
    this._user.set(user);
  }

  private loadUser(): AuthResponse | null {
    try {
      const raw = localStorage.getItem(USER_KEY);
      return raw ? (JSON.parse(raw) as AuthResponse) : null;
    } catch {
      return null;
    }
  }
}
