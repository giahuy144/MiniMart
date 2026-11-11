import { Component, computed, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { RouterModule } from '@angular/router';
import { CartService, StoreProduct } from '../../../core/services/cart.service';
@Component({
  selector: 'app-product-list',
  standalone: true,
  imports: [CommonModule, FormsModule, RouterModule],
  templateUrl: './product-list.component.html',
  styleUrls: ['./product-list.component.scss'],
})
export class ProductListComponent {
  readonly search = signal('');
  readonly selectedCategory = signal('Tất cả');
  readonly addedProduct = signal('');
  readonly categories = ['Tất cả', 'Đồ uống', 'Bánh tươi', 'Đồ ăn liền', 'Gia vị'];
  readonly products: StoreProduct[] = [
    {
      id: 'milk-1l',
      name: 'Sữa tươi tiệt trùng Vinamilk 100% 1L',
      category: 'Đồ uống',
      price: 36000,
      unit: 'Hộp 1L',
      mark: 'SỮA',
      tone: 'mint',
    },
    {
      id: 'bread',
      name: 'Bánh mì hoa cúc Harrys Pháp 500g',
      category: 'Bánh tươi',
      price: 125000,
      unit: 'Ổ 500g',
      mark: 'BÁNH',
      tone: 'peach',
    },
    {
      id: 'noodle',
      name: 'Mì Hảo Hảo tôm chua cay',
      category: 'Đồ ăn liền',
      price: 118000,
      unit: 'Thùng 30 gói',
      mark: 'MÌ',
      tone: 'rose',
    },
    {
      id: 'cola',
      name: 'Coca-Cola Không Đường 320ml',
      category: 'Đồ uống',
      price: 10500,
      unit: 'Lon 320ml',
      mark: 'COLA',
      tone: 'blue',
    },
    {
      id: 'sauce',
      name: 'Nước mắm cá cơm truyền thống 500ml',
      category: 'Gia vị',
      price: 42000,
      unit: 'Chai 500ml',
      mark: 'MẮM',
      tone: 'sand',
    },
    {
      id: 'oat',
      name: 'Sữa yến mạch nguyên chất 1L',
      category: 'Đồ uống',
      price: 39000,
      unit: 'Hộp 1L',
      mark: 'YẾN MẠCH',
      tone: 'sage',
    },
  ];
  readonly filteredProducts = computed(() => {
    const q = this.search().trim().toLocaleLowerCase('vi');
    return this.products.filter(
      (p) =>
        (this.selectedCategory() === 'Tất cả' || p.category === this.selectedCategory()) &&
        (!q || `${p.name} ${p.category}`.toLocaleLowerCase('vi').includes(q)),
    );
  });
  constructor(readonly cart: CartService) {}
  addToCart(p: StoreProduct): void {
    this.cart.add(p);
    this.addedProduct.set(p.id);
    setTimeout(() => {
      if (this.addedProduct() === p.id) this.addedProduct.set('');
    }, 1300);
  }
}
