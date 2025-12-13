import { Routes } from '@angular/router';

export const routes: Routes = [
  {
    path: '',
    redirectTo: 'login',
    pathMatch: 'full',
  },
  {
    path: 'auth',
    redirectTo: 'login',
    pathMatch: 'full',
  },
  {
    path: 'login',
    loadComponent: () =>
      import('../features/authorization/components/authorization-page/authorization-page').then(
        (m) => m.AuthorizationPage,
      ),
  },
  {
    path: 'features',
    loadComponent: () =>
      import('../features/features/components/features-page/features-page').then(
        (m) => m.FeaturesPage,
      ),
  },
  // Дополнительные маршруты...
];
