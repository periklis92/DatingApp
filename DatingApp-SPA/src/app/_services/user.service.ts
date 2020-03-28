import { Injectable } from '@angular/core';
import { environment } from 'src/environments/environment';
import { HttpClient, HttpHeaders, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';
import { User } from '../_models/user';
import { PaginatedResult } from '../_models/pagination';
import { map } from 'rxjs/operators';
import { Message } from '../_models/message';

@Injectable({
  providedIn: 'root'
})
export class UserService {
  baseUrl = environment.apiUrl;

  constructor(private http: HttpClient) { }

  getUsers(page?, itemsPerPage?, userParams?, likesParam?): Observable<PaginatedResult<User[]>> {
    const paginatedResult: PaginatedResult<User[]> = new PaginatedResult<User[]>();

    let params = new HttpParams();

    if (page != null) {
      params = params.append('pageNumber', page);
    }

    if (itemsPerPage != null) {
      params = params.append('pageSize', itemsPerPage);
    }

    if (userParams != null) {
      params = params.append('minAge', userParams.minAge);
      params = params.append('maxAge', userParams.maxAge);
      params = params.append('gender', userParams.gender);
      params = params.append('orderBy', userParams.orderBy);
    }

    if (likesParam === 'Likers') {
      params = params.append('likers', 'true');
    }

    if (likesParam === 'Likees') {
      params = params.append('likees', 'true');
    }

    return this.http.get<User[]>(this.baseUrl + 'user', { observe: 'response', params})
      .pipe (
        map (response => {
          paginatedResult.results = response.body;
          if (response.headers.get('Pagination') != null) {
            paginatedResult.pagination = JSON.parse(response.headers.get('Pagination'));
          }
          return paginatedResult;
        })
      );
  }

  getUser(id: number): Observable<User> {
    return this.http.get<User>(this.baseUrl + 'user/' + id);
  }

  updateUser(id: number, user: User) {
    return this.http.put(this.baseUrl + 'user/' + id, user);
  }

  setMainPhoto(userID: number, id: number) {
    return this.http.post(this.baseUrl + 'user/' + userID + '/photos/' + id + '/setMain', {});
  }

  deletePhoto(userID: number, id: number) {
    return this.http.delete(this.baseUrl + 'user/' + userID + '/photos/' + id);
  }

  sendLike(id: number, recipientID: number) {
    return this.http.post(this.baseUrl + 'user/' + id + '/like/' + recipientID, {});
  }

  getMessages(id: number, page?, itemsPerPage?, messageContainer?) {
    const paginatedResult: PaginatedResult<Message[]> = new PaginatedResult<Message[]>();

    let params = new HttpParams();

    params = params.append('MessageContainer', messageContainer);

    if (page != null) {
      params = params.append('pageNumber', page);
    }

    if (itemsPerPage != null) {
      params = params.append('pageSize', itemsPerPage);
    }

    return this.http.get<Message[]>(this.baseUrl + 'user/' + id + '/messages', {observe: 'response', params})
      .pipe(
        map(response => {
          paginatedResult.results = response.body;
          if (response.headers.get('Pagination') != null) {
            paginatedResult.pagination = JSON.parse(response.headers.get('Pagination'));
          }

          return paginatedResult;
        })
      );

  }

  getMessageThread(id: number, recipientID: number) {
    return this.http.get<Message[]>(this.baseUrl + 'user/' + id + '/messages/thread/' + recipientID);
  }

  sendMessage(id: number, message: Message) {
    return this.http.post(this.baseUrl + 'user/' + id + '/messages', message);
  }

  deleteMessage(id: number, userID: number) {
    return this.http.post(this.baseUrl + 'user/' + userID + '/messages/' + id, {});
  }

  markAsRead(userID: number, messageID: number) {
    console.log(this.baseUrl + 'user/' + userID + '/messages/' + messageID + '/read');
    this.http.post(this.baseUrl + 'user/' + userID + '/messages/' + messageID + '/read', {})
      .subscribe();
  }
}
