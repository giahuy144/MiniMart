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
  styleUrls: ['./register.component.scss'],
})
export class RegisterComponent {
  form = {
    username: '',
    fullName: '',
    email: '',
    phoneNumber: '',
    password: '',
    confirmPassword: '',
  };
  isLoading = signal(false);
  errorMsg = signal('');
  successMsg = signal('');
  constructor(
    private auth: AuthService,
    private router: Router,
  ) {}
  onSubmit(): void {
    this.errorMsg.set('');
    this.successMsg.set('');
    if (
      !this.form.username.trim() ||
      !this.form.fullName.trim() ||
      !this.form.email.trim() ||
      !this.form.phoneNumber.trim()
    ) {
      this.errorMsg.set('Vui lòng điền đầy đủ thông tin.');
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
      username: this.form.username.trim(),
      fullName: this.form.fullName.trim(),
      email: this.form.email.trim().toLowerCase(),
      phoneNumber: this.form.phoneNumber.trim(),
      password: this.form.password,
    };
    this.auth.register(payload).subscribe({
      next: (r) => {
        this.isLoading.set(false);
        if (r.success) {
          this.successMsg.set('Đăng ký thành công. Đang chuyển đến trang đăng nhập…');
          setTimeout(() => this.router.navigate(['/login']), 1600);
        } else this.errorMsg.set(r.message || 'Đăng ký thất bại.');
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
}
