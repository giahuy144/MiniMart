import { Component, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule, Router } from '@angular/router';
import { FormsModule } from '@angular/forms';
import { AuthService } from '../../../core/services/auth.service';
import { RegisterRequest } from '../../../core/models/auth.model';

@Component({
  selector: 'app-register',
  standalone: true,
  imports: [CommonModule, RouterModule, FormsModule],
  templateUrl: './register.component.html',
  styleUrls: ['./register.component.scss']
})
export class RegisterComponent {
  form = {
    username: '',
    fullName: '',
    email: '',
    phoneNumber: '',
    password: '',
    confirmPassword: ''
  };

  isLoading   = signal(false);
  errorMsg    = signal('');
  successMsg  = signal('');
  showPwd     = signal(false);

  constructor(private authService: AuthService, private router: Router) {}

  onSubmit(): void {
    this.errorMsg.set('');
    this.successMsg.set('');

    // Validate cơ bản
    if (!this.form.username.trim() || !this.form.fullName.trim() || !this.form.email.trim() || !this.form.phoneNumber.trim()) {
      this.errorMsg.set('Vui lòng điền đầy đủ các trường bắt buộc (*).');
      return;
    }

    if (this.form.password.length < 6) {
      this.errorMsg.set('Mật khẩu phải có ít nhất 6 ký tự.');
      return;
    }

    if (this.form.password !== this.form.confirmPassword) {
      this.errorMsg.set('Mật khẩu xác nhận không khớp.');
      return;
    }

    this.isLoading.set(true);

    const payload: RegisterRequest = {
      username:    this.form.username.trim(),
      fullName:    this.form.fullName.trim(),
      email:       this.form.email.trim().toLowerCase(),
      phoneNumber: this.form.phoneNumber.trim(),
      password:    this.form.password,
      role:        'Customer'
    };

    this.authService.register(payload).subscribe({
      next: (res) => {
        this.isLoading.set(false);
        if (res.success) {
          this.successMsg.set('🎉 Đăng ký thành công! Đang chuyển đến trang đăng nhập...');
          setTimeout(() => this.router.navigate(['/login']), 2000);
        } else {
          this.errorMsg.set(res.message || 'Đăng ký thất bại.');
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
