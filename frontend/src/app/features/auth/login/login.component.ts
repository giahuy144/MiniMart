import { Component, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule, Router } from '@angular/router';
import { FormsModule } from '@angular/forms';
import { AuthService } from '../../../core/services/auth.service';
import { LoginRequest } from '../../../core/models/auth.model';
@Component({
  selector: 'app-login',
  standalone: true,
  imports: [CommonModule, RouterModule, FormsModule],
  templateUrl: './login.component.html',
  styleUrls: ['./login.component.scss'],
})
export class LoginComponent {
  form: LoginRequest = { email: '', password: '' };
  isLoading = signal(false);
  errorMsg = signal('');
  showPwd = signal(false);
  constructor(
    private auth: AuthService,
    private router: Router,
  ) {}
  onSubmit(): void {
    this.errorMsg.set('');
    if (!this.form.email.trim() || !this.form.password.trim()) {
      this.errorMsg.set('Vui lòng nhập email và mật khẩu.');
      return;
    }
    this.isLoading.set(true);
    this.auth.login({ ...this.form, email: this.form.email.trim().toLowerCase() }).subscribe({
      next: (r) => {
        this.isLoading.set(false);
        if (r.success) {
          this.router.navigateByUrl(this.auth.homePathForRole(r.data?.role));
        } else this.errorMsg.set(r.message || 'Đăng nhập thất bại.');
      },
      error: (e) => {
        this.isLoading.set(false);
        this.errorMsg.set(
          e?.error?.message ||
            'Không thể kết nối đến máy chủ. Vui lòng kiểm tra Backend đang chạy.',
        );
      },
    });
  }
  togglePwd(): void {
    this.showPwd.update((v) => !v);
  }
}
