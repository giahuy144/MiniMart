import { Component, computed, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';

interface StockItem {
  id: string; sku: string; name: string; category: string;
  quantity: number; unit: string; minimum: number; updatedAt: string;
}

const STOCK_KEY = 'minimart_stock_items';

@Component({
  selector: 'app-stock-list',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './stock-list.component.html',
  styleUrls: ['./stock-list.component.scss']
})
export class StockListComponent {
  private readonly defaults: StockItem[] = [
    { id: 'milk-1l', sku: 'SKU-MILK-01', name: 'Sữa tươi tiệt trùng Vinamilk 100% 1L', category: 'Đồ uống', quantity: 18, unit: 'hộp', minimum: 20, updatedAt: '01/11/2026 21:15' },
    { id: 'noodle', sku: 'SKU-NOOD-05', name: 'Mì gói Hảo Hảo tôm chua cay', category: 'Đồ ăn liền', quantity: 145, unit: 'gói', minimum: 50, updatedAt: '01/11/2026 19:40' },
    { id: 'water', sku: 'SKU-WATER-02', name: 'Nước khoáng thiên nhiên Lavie 500ml', category: 'Đồ uống', quantity: 0, unit: 'chai', minimum: 30, updatedAt: '01/11/2026 18:22' },
    { id: 'apple', sku: 'SKU-FRUIT-01', name: 'Táo đỏ nhập khẩu', category: 'Trái cây', quantity: 12, unit: 'kg', minimum: 15, updatedAt: '01/11/2026 17:30' },
  ];
  readonly items = signal<StockItem[]>(this.loadItems());
  readonly receiptOpen = signal(false);
  readonly urgentReceipt = signal(false);
  readonly selectedProductId = signal('');
  readonly quantityToReceive = signal(1);
  readonly supplier = signal('');
  readonly receiptNote = signal('');
  readonly receivedCount = computed(() => this.items().length);
  readonly safeCount = computed(() => this.items().filter(item => item.quantity > item.minimum).length);
  readonly lowCount = computed(() => this.items().filter(item => item.quantity > 0 && item.quantity <= item.minimum).length);
  readonly outCount = computed(() => this.items().filter(item => item.quantity === 0).length);
  unitFor(id: string): string { return this.items().find(item => item.id === id)?.unit ?? 'đơn vị'; }

  openReceipt(item?: StockItem, urgent = false): void {
    this.selectedProductId.set(item?.id ?? '');
    this.quantityToReceive.set(1);
    this.supplier.set('');
    this.receiptNote.set('');
    this.urgentReceipt.set(urgent);
    this.receiptOpen.set(true);
  }

  submitReceipt(): void {
    const id = this.selectedProductId();
    const amount = Number(this.quantityToReceive());
    if (!id || !Number.isInteger(amount) || amount < 1) return;
    const timestamp = new Date().toLocaleString('vi-VN', { dateStyle: 'short', timeStyle: 'short' });
    const updated = this.items().map(item => item.id === id
      ? { ...item, quantity: item.quantity + amount, updatedAt: timestamp }
      : item);
    this.items.set(updated);
    localStorage.setItem(STOCK_KEY, JSON.stringify(updated));
    const receipts = JSON.parse(localStorage.getItem('minimart_stock_receipts') ?? '[]') as unknown[];
    receipts.unshift({ productId: id, quantity: amount, supplier: this.supplier().trim(), note: this.receiptNote().trim(), urgent: this.urgentReceipt(), createdAt: timestamp });
    localStorage.setItem('minimart_stock_receipts', JSON.stringify(receipts));
    this.receiptOpen.set(false);
  }

  private loadItems(): StockItem[] {
    try {
      const saved = localStorage.getItem(STOCK_KEY);
      return saved ? JSON.parse(saved) as StockItem[] : this.defaults;
    } catch { return this.defaults; }
  }
}
