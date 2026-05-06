import { Component, inject, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { Router } from '@angular/router';
import { HttpErrorResponse } from '@angular/common/http';
import { AccountService } from '../_services/account.service';

@Component({
  selector: 'app-register',
  standalone: true,
  imports: [FormsModule],
  templateUrl: './register.html',
  styleUrl: './register.css'
})
export class Register {
  model: any = {};
  accountService = inject(AccountService);
  router = inject(Router);
  errorMessage = signal('');
  userNameServerError = signal('');

  register() {
    this.errorMessage.set('');
    this.userNameServerError.set('');

    if (this.model.password !== this.model.confirmPassword) {
      this.errorMessage.set('Passwords do not match. Please try again.');
      return;
    }

    const datatosend = {
      firstName: this.model.firstName,
      lastName: this.model.lastName,
      userName: this.model.userName,
      emailAddress: this.model.emailAddress,
      password: this.model.password,
      role: this.model.role
    };

    this.accountService.register(datatosend).subscribe({
      next: () => {
        this.router.navigateByUrl('/');
      },
      error: (err: HttpErrorResponse) => {
        const serverMessage = err.error?.message ?? 'Registration failed. Please try again.';
        if (err.status === 409) {
          this.userNameServerError.set(serverMessage);
          return;
        }
        this.errorMessage.set(serverMessage);
      }
    });
  }
}
