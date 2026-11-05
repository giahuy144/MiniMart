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
  styleUrls: ['./login.component.scss']
})
export class LoginComponent {
  form: LoginRequest = { username: '', password: '' };

  isLoading = signal(false);
  errorMsg  = signal('');
  showPwd   = signal(false);

  // Danh sách tài khoản demo nhanh
  demoAccounts = [
    { label: 'Khách hàng',    username: 'customer',    password: 'customer123', role: 'Customer' },
    { label: 'Thu ngân',      username: 'cashier',     password: 'cashier123',  role: 'Cashier' },
    { label: 'Thủ kho',       username: 'stockkeeper', password: 'stock123',    role: 'Stockkeeper' },
    { label: 'Quản lý/Admin', username: 'admin',       password: 'admin123',    role: 'Admin' },
  ];
  selectedDemo = this.demoAccounts[0];

  constructor(private authService: AuthService, private router: Router) {}

  selectDemo(account: typeof this.demoAccounts[0]): void {
    this.selectedDemo = account;
    this.form.username = account.username;
    this.form.password = account.password;
    this.errorMsg.set('');
  }

  onSubmit(): void {
    this.errorMsg.set('');

    if (!this.form.username.trim() || !this.form.password.trim()) {
      this.errorMsg.set('Vui lòng nhập tên đăng nhập và mật khẩu.');
      return;
    }

    this.isLoading.set(true);

    this.authService.login(this.form).subscribe({
      next: (res) => {
        this.isLoading.set(false);
        if (res.success) {
          // Điều hướng theo vai trò
          const role = res.data?.role;
          if (role === 'Admin' || role === 'Stockkeeper') {
            this.router.navigate(['/inventory']);
          } else if (role === 'Cashier') {
            this.router.navigate(['/pos']);
          } else {
            this.router.navigate(['/products']);
          }
        } else {
          this.errorMsg.set(res.message || 'Đăng nhập thất bại.');
        }
      },
      error: (err) => {
        this.isLoading.set(false);
        const msg = err?.error?.message;
        this.errorMsg.set(msg || 'Không thể kết nối đến máy chủ. Vui lòng kiểm tra Backend đang chạy.');
      }
    });
  }

  togglePwd(): void {
    this.showPwd.update(v => !v);
  }
}
