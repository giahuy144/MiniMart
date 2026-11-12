import { inject } from '@angular/core';
import { CanActivateFn, Router } from '@angular/router';
import { AuthService } from '../services/auth.service';

export const authGuard: CanActivateFn = (route, state) => {
  const auth   = inject(AuthService);
  const router = inject(Router);

  if (auth.isLoggedIn()) {
    return true;
  }

  return router.createUrlTree(['/login'], { queryParams: { returnUrl: state.url } });
};

export const guestGuard: CanActivateFn = () => {
  const auth = inject(AuthService);
  const router = inject(Router);
  return auth.isLoggedIn() ? router.parseUrl(auth.homePathForRole()) : true;
};

export const homeRedirectGuard: CanActivateFn = () => {
  const auth = inject(AuthService);
  const router = inject(Router);
  return router.parseUrl(auth.homePathForRole());
};

export const roleGuard: CanActivateFn = (route) => {
  const auth = inject(AuthService);
  const router = inject(Router);
  const allowedRoles = route.data['roles'] as string[] | undefined;
  return !allowedRoles || allowedRoles.includes(auth.userRole())
    ? true
    : router.parseUrl(auth.homePathForRole());
};
