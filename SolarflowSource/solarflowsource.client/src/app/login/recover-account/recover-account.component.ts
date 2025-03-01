import { Component } from "@angular/core";
import { Router } from '@angular/router';
import { FormsModule } from '@angular/forms';
import { AuthenticationService } from '../../services/authentication.service';

@Component({
  selector: "app-recover-account",
  templateUrl: "./recover-account.component.html",
  styleUrls: ["./recover-account.component.css"],
  imports: [FormsModule]
})
export class RecoverAccountComponent {
  email: string = "";

  constructor(private router: Router, private authenticationService: AuthenticationService) { }

  goBackToLogin(): void {
    this.router.navigate(['/']);
  }

  recoverAccount(): void {
    this.authenticationService.recoverAccount(this.email).subscribe(
      (response) => {
        alert('Account recovery email sent.');  // Display success message
      },
      (error) => {
        alert('Failed to recover account: ' + error.error);  // Display error message
      }
    );
  }
}
