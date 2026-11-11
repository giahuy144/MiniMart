import { Injectable, computed, signal } from '@angular/core';
export interface StoreProduct {
  id: string;
  name: string;
  category: string;
  price: number;
  unit: string;
  mark: string;
  tone: string;
}
export interface CartItem extends StoreProduct {
  quantity: number;
}
const KEY = 'minimart_cart';
@Injectable({ providedIn: 'root' })
export class CartService {
  private readonly stored = signal<CartItem[]>(this.read());
  readonly items = this.stored.asReadonly();
  readonly itemCount = computed(() => this.stored().reduce((sum, item) => sum + item.quantity, 0));
  readonly subtotal = computed(() =>
    this.stored().reduce((sum, item) => sum + item.price * item.quantity, 0),
  );
  add(product: StoreProduct): void {
    const items = this.stored(),
      old = items.find((i) => i.id === product.id);
    this.save(
      old
        ? items.map((i) => (i.id === product.id ? { ...i, quantity: i.quantity + 1 } : i))
        : [...items, { ...product, quantity: 1 }],
    );
  }
  setQuantity(id: string, quantity: number): void {
    if (quantity < 1) return this.remove(id);
    this.save(this.stored().map((i) => (i.id === id ? { ...i, quantity } : i)));
  }
  remove(id: string): void {
    this.save(this.stored().filter((i) => i.id !== id));
  }
  private save(items: CartItem[]): void {
    this.stored.set(items);
    localStorage.setItem(KEY, JSON.stringify(items));
  }
  private read(): CartItem[] {
    try {
      const v = localStorage.getItem(KEY);
      return v ? (JSON.parse(v) as CartItem[]) : [];
    } catch {
      return [];
    }
  }
}
