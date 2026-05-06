import { HttpClient } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { map, Observable } from 'rxjs';
import {
  ClientDto,
  CreateClientRequest,
  PagedResult,
  UpdateClientRequest
} from './clients.models';
import { API_BASE_URL } from '../api-base-url.token';

@Injectable({ providedIn: 'root' })
export class ClientsApiService {
  private readonly clientsUrl = `${inject(API_BASE_URL)}/api/clients`;

  constructor(private readonly http: HttpClient) {}

  getClients(page = 1, pageSize = 20): Observable<PagedResult<ClientDto>> {
    return this.http.get<PagedResult<ClientDto>>(this.clientsUrl, {
      params: { page, pageSize }
    });
  }

  getClient(id: string): Observable<ClientDto> {
    return this.http
      .get<ClientDto>(`${this.clientsUrl}/${id}`)
      .pipe(map((client) => this.normalizeClient(client)));
  }

  createClient(payload: CreateClientRequest): Observable<ClientDto> {
    return this.http
      .post<ClientDto>(this.clientsUrl, this.normalizeRequest(payload))
      .pipe(map((client) => this.normalizeClient(client)));
  }

  updateClient(id: string, payload: UpdateClientRequest): Observable<ClientDto> {
    return this.http
      .put<ClientDto>(`${this.clientsUrl}/${id}`, this.normalizeRequest(payload))
      .pipe(map((client) => this.normalizeClient(client)));
  }

  private normalizeRequest<T extends CreateClientRequest>(payload: T): T {
    return {
      ...payload,
      name: payload.name.trim(),
      taxId: payload.taxId.trim(),
      email: payload.email?.trim() || null,
      phone: payload.phone?.trim() || null,
      address: payload.address?.trim() || null
    };
  }

  private normalizeClient(client: ClientDto): ClientDto {
    return {
      ...client,
      email: client.email ?? null,
      phone: client.phone ?? null,
      address: client.address ?? null
    };
  }
}
