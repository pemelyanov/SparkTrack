import { Component, signal, computed, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import {
  FormsModule,
  ReactiveFormsModule,
  FormBuilder,
  FormGroup,
  Validators,
} from '@angular/forms';
import { Router, RouterModule } from '@angular/router';

// Angular Material Standalone imports
import { MatCardModule } from '@angular/material/card';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatCheckboxModule } from '@angular/material/checkbox';

@Component({
  selector: 'app-authorization-page',
  standalone: true,
  imports: [
    CommonModule,
    FormsModule,
    ReactiveFormsModule,
    RouterModule,

    // Material Standalone Modules
    MatCardModule,
    MatFormFieldModule,
    MatInputModule,
    MatButtonModule,
    MatIconModule,
    MatProgressSpinnerModule,
    MatCheckboxModule,
  ],
  templateUrl: './authorization-page.html',
  styleUrls: ['./authorization-page.scss'],
})
export class AuthorizationPage {
  private formBuilder = inject(FormBuilder);
  private router = inject(Router);

  // Signals
  public hidePassword = signal(true);
  public isLoading = signal(false);

  // Reactive Form
  public loginForm: FormGroup = this.formBuilder.group({
    login: ['', [Validators.required, Validators.email]],
    password: ['', [Validators.required, Validators.minLength(6)]],
    rememberMe: [false],
  });

  // Computed signals для удобства
  public loginControl = computed(() => this.loginForm.get('login'));
  public passwordControl = computed(() => this.loginForm.get('password'));

  public isFormValid = computed(() => this.loginForm.valid);
  public showLoginError = computed(
    () => this.loginControl()?.touched && this.loginControl()?.invalid,
  );

  public showPasswordError = computed(
    () => this.passwordControl()?.touched && this.passwordControl()?.invalid,
  );

  // Переключение видимости пароля
  public togglePasswordVisibility(): void {
    this.hidePassword.update((current) => !current);
  }

  // Обработка отправки формы
  public onSubmit(): void {
    if (this.loginForm.valid) {
      this.isLoading.set(true);

      // Имитация запроса на сервер
      setTimeout(() => {
        this.isLoading.set(false);
        console.log('Форма отправлена:', this.loginForm.value);
        // Здесь будет реальная авторизация
        this.router.navigate(['/features']);
      }, 500);
    } else {
      this.markFormGroupTouched(this.loginForm);
    }
  }

  // Сброс формы
  public onReset(): void {
    this.loginForm.reset({
      login: '',
      password: '',
      rememberMe: false,
    });
    this.hidePassword.set(true);
  }

  // Вспомогательный метод для пометки всех полей формы
  private markFormGroupTouched(formGroup: FormGroup): void {
    Object.values(formGroup.controls).forEach((control) => {
      control.markAsTouched();
      if (control instanceof FormGroup) {
        this.markFormGroupTouched(control);
      }
    });
  }
}
