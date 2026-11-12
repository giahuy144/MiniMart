import { Component } from '@angular/core';
import { RouterLink, RouterLinkActive, RouterOutlet } from '@angular/router';
import { AuthService } from './core/services/auth.service';
import { CartService } from './core/services/cart.service';
@Component({
  selector: 'app-root',
  imports: [RouterOutlet, RouterLink, RouterLinkActive],
  templateUrl: './app.html',
  styleUrl: './app.scss',
})
export class App {
  constructor(
    readonly auth: AuthService,
    readonly cart: CartService,
  ) {}

  roleLabel(): string {
    switch (this.auth.userRole()) {
      case 'Admin': return 'Quản trị viên';
      case 'Cashier': return 'Thu ngân';
      case 'Stockkeeper': return 'Nhân viên kho';
      case 'Customer': return 'Khách hàng';
      default: return '';
    }
  }
}
