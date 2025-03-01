import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';

@Injectable({
  providedIn: 'root',
})
export class AuthenticationService {
  private apiUrl = 'https://localhost:7212/api/user/';

  constructor(private http: HttpClient) { }

  authenticate(email: string, password: string): Observable<any> {
    return this.http.post(this.apiUrl + "authenticate", { email, password });
  }

  recoverAccount(email: string): Observable<any> {
    return this.http.post(this.apiUrl + "recover-account", { email });
  }
}
