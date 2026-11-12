import { Injectable, computed, signal } from '@angular/core';
export interface StoreProduct {
  id: string;
  name: string;
  category: string;
  price: number;
  unit: string;
  mark: string;
  tone: string;
  imageUrl?: string;
  description?: string;
}
export interface CartItem extends StoreProduct {
  quantity: number;
}
const KEY = 'minimart_cart';
const POS_KEY = 'minimart_pos_cart';
@Injectable({ providedIn: 'root' })
export class CartService {
  private readonly stored = signal<CartItem[]>(this.read());
  private readonly posStored = signal<CartItem[]>(this.read(POS_KEY));
  readonly items = this.stored.asReadonly();
  readonly posItems = this.posStored.asReadonly();
  readonly itemCount = computed(() => this.stored().reduce((sum, item) => sum + item.quantity, 0));
  readonly subtotal = computed(() =>
    this.stored().reduce((sum, item) => sum + item.price * item.quantity, 0),
  );
  readonly posItemCount = computed(() => this.posStored().reduce((sum, item) => sum + item.quantity, 0));
  readonly posSubtotal = computed(() => this.posStored().reduce((sum, item) => sum + item.price * item.quantity, 0));
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
  addToPos(product: StoreProduct): void {
    const old = this.posStored().find((item) => item.id === product.id);
    this.savePos(old
      ? this.posStored().map((item) => item.id === product.id ? { ...item, quantity: item.quantity + 1 } : item)
      : [...this.posStored(), { ...product, quantity: 1 }]);
  }
  setPosQuantity(id: string, quantity: number): void {
    if (quantity < 1) return this.removeFromPos(id);
    this.savePos(this.posStored().map((item) => item.id === id ? { ...item, quantity } : item));
  }
  removeFromPos(id: string): void { this.savePos(this.posStored().filter((item) => item.id !== id)); }
  clearPos(): void { this.savePos([]); }
  private save(items: CartItem[]): void {
    this.stored.set(items);
    localStorage.setItem(KEY, JSON.stringify(items));
  }
  private savePos(items: CartItem[]): void {
    this.posStored.set(items);
    localStorage.setItem(POS_KEY, JSON.stringify(items));
  }
  private read(key = KEY): CartItem[] {
    try {
      const v = localStorage.getItem(key);
      return v ? (JSON.parse(v) as CartItem[]) : [];
    } catch {
      return [];
    }
  }
}
