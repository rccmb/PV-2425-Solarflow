import { Component } from "@angular/core";
import { Router } from '@angular/router';
import { FormsModule } from '@angular/forms';

@Component({
  selector: "app-recover-account",
  templateUrl: "./recover-account.component.html",
  styleUrls: ["./recover-account.component.css"],
  imports: [FormsModule]
})
export class RecoverAccountComponent {
  username: string = "";

  constructor(private router: Router) { }

  goBackToLogin(): void {
    this.router.navigate(['/']);
  }

  recoverAccount(): void {
    
  }
}
