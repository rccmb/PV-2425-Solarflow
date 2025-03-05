import { Component } from '@angular/core';
import { Router } from '@angular/router';
import { AuthenticationService } from '../../services/authentication.service';
import { FormsModule } from '@angular/forms';

@Component({
  selector: 'app-register-account',
  templateUrl: './register-account.component.html',
  styleUrls: ['./register-account.component.css'],
  imports: [FormsModule]
})
export class RegisterAccountComponent {
  name: string = "";
  email: string = "";
  password: string = "";
  confirmPassword: string = "";

  constructor(private authenticationService: AuthenticationService, private router: Router) { }

  register() {
    if (this.password !== this.confirmPassword) {
      alert("Passwords do not match!");
      return;
    }

    this.authenticationService.registerAccount(this.name, this.email, this.password).subscribe(
      (response) => {
        alert(response.message);
        this.router.navigate(['']);
      },
      (error) => {
        alert(error.error.message || 'Failed to register!');
      }
    );
  }

  back() {
    this.router.navigate(['']);
  }
}
