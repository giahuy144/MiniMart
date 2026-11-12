import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';
import { CartService } from '../../../core/services/cart.service';

@Component({
  selector: 'app-pos-checkout',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './pos-checkout.component.html',
  styleUrls: ['./pos-checkout.component.scss']
})
export class PosCheckoutComponent {
  constructor(readonly cart: CartService) {}

  setQuantity(id: string, quantity: number): void { this.cart.setPosQuantity(id, quantity); }
  remove(id: string): void { this.cart.removeFromPos(id); }
  checkout(): void {
    if (this.cart.posItems().length === 0) return;
    window.alert('Đã ghi nhận thanh toán tiền mặt cho đơn hàng.');
    this.cart.clearPos();
  }
}
