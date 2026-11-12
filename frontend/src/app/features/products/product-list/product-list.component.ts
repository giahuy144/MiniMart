import { Component, computed, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { RouterModule } from '@angular/router';
import { CartService, StoreProduct } from '../../../core/services/cart.service';
import { AuthService } from '../../../core/services/auth.service';
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
  readonly categories = ['Tất cả', 'Đồ uống', 'Bánh tươi', 'Đồ ăn liền', 'Gia vị', 'Trái cây'];
  private readonly defaultProducts: StoreProduct[] = [
    {
      id: 'milk-1l',
      name: 'Sữa tươi tiệt trùng Vinamilk 100% 1L',
      category: 'Đồ uống',
      price: 36000,
      unit: 'Hộp 1L',
      mark: 'SỮA',
      tone: 'mint',
      description: 'Sữa tươi tiệt trùng nguyên chất, thơm ngon và tiện lợi cho cả gia đình.',
    },
    {
      id: 'bread',
      name: 'Bánh mì hoa cúc Harrys Pháp 500g',
      category: 'Bánh tươi',
      price: 125000,
      unit: 'Ổ 500g',
      mark: 'BÁNH',
      tone: 'peach',
      description: 'Bánh mì hoa cúc mềm thơm, thích hợp dùng cho bữa sáng hoặc bữa phụ.',
    },
    {
      id: 'noodle',
      name: 'Mì Hảo Hảo tôm chua cay',
      category: 'Đồ ăn liền',
      price: 118000,
      unit: 'Thùng 30 gói',
      mark: 'MÌ',
      tone: 'rose',
      description: 'Mì ăn liền Hảo Hảo vị tôm chua cay, đóng thùng tiết kiệm.',
    },
    {
      id: 'cola',
      name: 'Coca-Cola Không Đường 320ml',
      category: 'Đồ uống',
      price: 10500,
      unit: 'Lon 320ml',
      mark: 'COLA',
      tone: 'blue',
      description: 'Coca-Cola không đường lon 320ml, giải khát mát lạnh.',
    },
    {
      id: 'sauce',
      name: 'Nước mắm cá cơm truyền thống 500ml',
      category: 'Gia vị',
      price: 42000,
      unit: 'Chai 500ml',
      mark: 'MẮM',
      tone: 'sand',
      description: 'Nước mắm cá cơm truyền thống đậm đà, chai 500ml.',
    },
    {
      id: 'oat',
      name: 'Sữa yến mạch nguyên chất 1L',
      category: 'Đồ uống',
      price: 39000,
      unit: 'Hộp 1L',
      mark: 'YẾN MẠCH',
      tone: 'sage',
      description: 'Sữa yến mạch nguyên chất, lựa chọn nhẹ nhàng cho mỗi ngày.',
    },
    {
      id: 'fruit-apple', name: 'Táo đỏ nhập khẩu', category: 'Trái cây', price: 68000,
      unit: 'Kg', mark: 'TÁO', tone: 'rose', description: 'Táo đỏ giòn ngọt, phù hợp ăn tươi hoặc dùng trong món tráng miệng.',
    },
    {
      id: 'fruit-orange', name: 'Cam vàng mọng nước', category: 'Trái cây', price: 42000,
      unit: 'Kg', mark: 'CAM', tone: 'peach', description: 'Cam tươi vị ngọt dịu, giàu vitamin C.',
    },
    {
      id: 'fruit-grape', name: 'Nho xanh không hạt', category: 'Trái cây', price: 99000,
      unit: 'Hộp 500g', mark: 'NHO', tone: 'sage', description: 'Nho xanh không hạt, ngọt thanh và dễ dùng.',
    },
  ];
  readonly products = signal<StoreProduct[]>(this.loadProducts());
  readonly selectedProduct = signal<StoreProduct | null>(null);
  readonly showAddForm = signal(false);
  readonly imagePreview = signal('');
  readonly isAdmin = computed(() => this.auth.userRole() === 'Admin');
  newProduct: StoreProduct = this.emptyProduct();
  readonly filteredProducts = computed(() => {
    const q = this.search().trim().toLocaleLowerCase('vi');
    return this.products().filter(
      (p) =>
        (this.selectedCategory() === 'Tất cả' || p.category === this.selectedCategory()) &&
        (!q || `${p.name} ${p.category}`.toLocaleLowerCase('vi').includes(q)),
    );
  });
  constructor(readonly cart: CartService, readonly auth: AuthService) {}
  addToCart(p: StoreProduct): void {
    if (this.auth.userRole() === 'Cashier') this.cart.addToPos(p);
    else this.cart.add(p);
    this.addedProduct.set(p.id);
    setTimeout(() => {
      if (this.addedProduct() === p.id) this.addedProduct.set('');
    }, 1300);
  }

  openAddForm(): void {
    this.newProduct = this.emptyProduct();
    this.imagePreview.set('');
    this.showAddForm.set(true);
  }

  onImageSelected(event: Event): void {
    const input = event.target as HTMLInputElement;
    const file = input.files?.[0];
    if (!file) return;
    if (!file.type.startsWith('image/')) return;
    const reader = new FileReader();
    reader.onload = () => {
      const image = String(reader.result ?? '');
      this.imagePreview.set(image);
      this.newProduct = { ...this.newProduct, imageUrl: image };
    };
    reader.readAsDataURL(file);
  }

  saveProduct(): void {
    const product = this.newProduct;
    if (!product.name.trim() || !product.category.trim() || !product.unit.trim() || product.price <= 0) return;
    const saved = { ...product, id: `custom-${Date.now()}`, mark: product.name.trim().slice(0, 12), tone: 'mint' };
    const updated = [saved, ...this.products()];
    this.products.set(updated);
    localStorage.setItem('minimart_products', JSON.stringify(updated));
    this.showAddForm.set(false);
  }

  private emptyProduct(): StoreProduct {
    return { id: '', name: '', category: '', price: 0, unit: '', mark: '', tone: 'mint', description: '' };
  }

  private loadProducts(): StoreProduct[] {
    try {
      const saved = localStorage.getItem('minimart_products');
      return saved ? [...JSON.parse(saved) as StoreProduct[], ...this.defaultProducts] : this.defaultProducts;
    } catch { return this.defaultProducts; }
  }
}
