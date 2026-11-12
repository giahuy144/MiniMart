import { CommonModule, CurrencyPipe, DatePipe } from '@angular/common';
import { HttpClient } from '@angular/common/http';
import { Component, OnInit, signal } from '@angular/core';
import { environment } from '../../../../environments/environment';
import { AuthService } from '../../../core/services/auth.service';

interface CustomerResponse { success: boolean; data: { id: number } | null; }
interface OrderSummary {
  id: string; orderNumber: string; status: string; paymentStatus: string;
  totalAmount: number; createdAt: string;
  details: Array<{ productName: string; quantity: number }>;
}
interface OrdersResponse { success: boolean; data: OrderSummary[] | null; }

@Component({
  selector: 'app-order-history', standalone: true,
  imports: [CommonModule, CurrencyPipe, DatePipe],
  template: `
    <section class="orders-page">
      <header><p class="eyebrow">TÀI KHOẢN CỦA BẠN</p><h1>Lịch sử đơn hàng</h1><p>Theo dõi các đơn hàng bạn đã đặt tại MiniMart.</p></header>
      @if (loading()) { <div class="message">Đang tải đơn hàng…</div> }
      @else if (error()) { <div class="message error">{{ error() }}</div> }
      @else if (orders().length === 0) { <div class="empty"><h2>Bạn chưa có đơn hàng nào</h2><p>Các đơn hàng của bạn sẽ xuất hiện tại đây sau khi đặt hàng.</p></div> }
      @else { <div class="order-list">@for (order of orders(); track order.id) {
        <article class="order-card"><div class="order-heading"><div><span class="label">MÃ ĐƠN</span><strong>{{ order.orderNumber }}</strong></div><span class="status">{{ order.status }}</span></div>
          <p class="date">{{ order.createdAt | date:'dd/MM/yyyy, HH:mm' }}</p><ul>@for (item of order.details; track $index) { <li>{{ item.productName }} <span>× {{ item.quantity }}</span></li> }</ul>
          <footer><span>{{ order.paymentStatus }}</span><strong>{{ order.totalAmount | currency:'VND':'symbol':'1.0-0':'vi-VN' }}</strong></footer></article>
      }</div>}
    </section>
  `,
  styles: [`
    .orders-page{max-width:960px;margin:0 auto;padding:48px 24px 72px;color:#24352a}header{margin-bottom:30px}.eyebrow,.label{font-size:11px;letter-spacing:.12em;color:#6c7a70;font-weight:700}h1{font:500 34px Georgia,serif;margin:8px 0}header p:last-child,.date{color:#6c7a70}.message,.empty{padding:44px 24px;text-align:center;background:#fff;border:1px solid #e3e9e3}.empty h2{font:500 22px Georgia,serif}.order-list{display:grid;gap:14px}.order-card{background:#fff;border:1px solid #e3e9e3;padding:22px 24px}.order-heading,footer{display:flex;justify-content:space-between;align-items:center}.order-heading>div{display:grid;gap:6px}.status{color:#2f6744;background:#e8f1e9;padding:6px 10px;font-size:12px}.date{font-size:13px;margin:14px 0}.order-card ul{padding:0;list-style:none;border-top:1px solid #edf0ec}.order-card li{display:flex;justify-content:space-between;padding:10px 0;border-bottom:1px solid #edf0ec;font-size:14px}.order-card li span{color:#6c7a70}.order-card footer{padding-top:15px;color:#6c7a70;font-size:13px}.order-card footer strong{font-size:17px;color:#24352a}.error{color:#a2362e}
  `],
})
export class OrderHistoryComponent implements OnInit {
  readonly orders = signal<OrderSummary[]>([]);
  readonly loading = signal(true);
  readonly error = signal('');

  constructor(private readonly http: HttpClient, private readonly auth: AuthService) {}

  ngOnInit(): void {
    const userId = this.auth.currentUser()?.userId;
    if (!userId) { this.fail('Không tìm thấy thông tin tài khoản. Vui lòng đăng nhập lại.'); return; }
    this.http.get<CustomerResponse>(`${environment.apiUrl}/api/customers/by-user/${encodeURIComponent(userId)}`)
      .subscribe({
        next: (customer) => {
          if (!customer.success || !customer.data) { this.orders.set([]); this.loading.set(false); return; }
          this.http.get<OrdersResponse>(`${environment.apiUrl}/api/orders`, { params: { customerId: customer.data.id, page: 1, pageSize: 50 } })
            .subscribe({
              next: (response) => { this.orders.set(response.success ? (response.data ?? []) : []); this.loading.set(false); },
              error: () => this.fail('Không thể tải lịch sử đơn hàng. Vui lòng thử lại sau.'),
            });
        },
        error: (response) => {
          if (response.status === 404) { this.orders.set([]); this.loading.set(false); }
          else this.fail('Không thể tải thông tin khách hàng. Vui lòng thử lại sau.');
        },
      });
  }
  private fail(message: string): void { this.error.set(message); this.loading.set(false); }
}
