import { Routes } from '@angular/router';
import { Home } from './home/home';
import { Login } from './login/login';
import { Register } from './register/register';
import { SubjectManagement } from './subject-management/subject-management';

export const routes: Routes = [
  { path: '', component: Home },
  { path: 'login', component: Login },
  { path: 'register', component: Register },
  { path: 'mentor/subjects', component: SubjectManagement},
  { path: 'home', redirectTo: '', pathMatch: 'full' },
  { path: '**', redirectTo: '' }
];
