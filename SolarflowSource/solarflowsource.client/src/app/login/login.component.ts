import { Component } from '@angular/core';
import { Router } from '@angular/router';
import { AuthenticationService } from '../services/authentication.service';
import { FormsModule } from '@angular/forms';

@Component({
  selector: 'app-login',
  templateUrl: './login.component.html',
  styleUrls: ['./login.component.css'],
  imports: [FormsModule]
})
export class LoginComponent {
  email: string = "";
  password: string = "";

  constructor(private authenticationService: AuthenticationService, private router: Router) { }

  login() {
    this.authenticationService.authenticate(this.email, this.password).subscribe(
      (response) => {
        localStorage.setItem('authToken', response.token);
        this.router.navigate(['/dashboard']);
      },
      (error) => {
        alert('Failed to login!');  // Display error message
      }
    )
  }

  recoverAccount() {
    this.router.navigate(['login/recover-account']);
  }
}
