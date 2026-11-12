import { Routes } from '@angular/router';
import { authGuard, guestGuard, homeRedirectGuard, roleGuard } from './core/guards/auth.guard';
export const routes: Routes = [
  { path: '', pathMatch: 'full', canActivate: [homeRedirectGuard], children: [] },
  {
    path: 'login',
    canActivate: [guestGuard],
    loadComponent: () =>
      import('./features/auth/login/login.component').then((m) => m.LoginComponent),
  },
  {
    path: 'register',
    canActivate: [guestGuard],
    loadComponent: () =>
      import('./features/auth/register/register.component').then((m) => m.RegisterComponent),
  },
  {
    path: 'products',
    canActivate: [authGuard, roleGuard], data: { roles: ['Customer', 'Cashier', 'Admin'] },
    loadComponent: () =>
      import('./features/products/product-list/product-list.component').then(
        (m) => m.ProductListComponent,
      ),
  },
  {
    path: 'cart',
    canActivate: [authGuard, roleGuard], data: { roles: ['Customer'] },
    loadComponent: () => import('./features/cart/cart.component').then((m) => m.CartComponent),
  },
  {
    path: 'orders',
    canActivate: [authGuard, roleGuard], data: { roles: ['Customer'] },
    loadComponent: () =>
      import('./features/orders/order-history/order-history.component').then(
        (m) => m.OrderHistoryComponent,
      ),
  },
  {
    path: 'pos',
    canActivate: [authGuard, roleGuard], data: { roles: ['Cashier', 'Admin'] },
    loadComponent: () =>
      import('./features/pos/pos-checkout/pos-checkout.component').then(
        (m) => m.PosCheckoutComponent,
      ),
  },
  {
    path: 'inventory',
    canActivate: [authGuard, roleGuard], data: { roles: ['Stockkeeper', 'Admin'] },
    loadComponent: () =>
      import('./features/inventory/stock-list/stock-list.component').then(
        (m) => m.StockListComponent,
      ),
  },
  {
    path: 'reports',
    canActivate: [authGuard, roleGuard], data: { roles: ['Admin'] },
    loadComponent: () =>
      import('./features/reports/dashboard/dashboard.component').then((m) => m.DashboardComponent),
  },
  { path: '**', redirectTo: 'products' },
];
