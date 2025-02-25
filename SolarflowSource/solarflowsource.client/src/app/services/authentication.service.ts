import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';

@Injectable({
  providedIn: 'root',
})
export class AuthenticationService {
  private apiUrl = 'https://localhost:7212/api/user/';

  constructor(private http: HttpClient) { }

  authenticate(username: string, password: string): Observable<any> {
    return this.http.post(this.apiUrl + "authenticate", { username, password });
  }

  recoverAccount(username: string): Observable<any> {
    return this.http.post(this.apiUrl + "recover-account", { username });
  }
}
