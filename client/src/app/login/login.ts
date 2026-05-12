import { Component, inject, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { Router } from '@angular/router';
import { HttpErrorResponse } from '@angular/common/http';
import { AccountService } from '../_services/account.service';

@Component({
  selector: 'app-login',
  standalone: true,
  imports: [FormsModule],
  templateUrl: './login.html',
  styleUrl: './login.css'
})
export class Login {
  private accountService = inject(AccountService);
  private router = inject(Router);

  model: any = {};
  errorMessage = signal('');
  isLoading = signal(false);

  login() {
    this.errorMessage.set('');
    this.isLoading.set(true);
    this.accountService.login(this.model).subscribe({
      next: (user) => {
        this.isLoading.set(false);
        if (user?.role?.toLowerCase().includes('mentor')) {
          this.router.navigateByUrl('/mentor/subjects');
        } else {
          this.router.navigateByUrl('/');
        }
      },
      error: (err: HttpErrorResponse) => {
        const error = err.error?.message ?? 'Login failed. Please try again.';
        this.errorMessage.set(error);
        this.isLoading.set(false);
      }
    });
  }
}
