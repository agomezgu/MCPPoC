import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { map, Observable } from 'rxjs';
import { ClientDto, CreateClientRequest, PagedResult } from './clients.models';

@Injectable({ providedIn: 'root' })
export class ClientsApiService {
  private readonly clientsUrl = '/api/clients';

  constructor(private readonly http: HttpClient) {}

  getClients(page = 1, pageSize = 20): Observable<PagedResult<ClientDto>> {
    return this.http.get<PagedResult<ClientDto>>(this.clientsUrl, {
      params: { page, pageSize }
    });
  }

  createClient(payload: CreateClientRequest): Observable<ClientDto> {
    const normalizedPayload: CreateClientRequest = {
      ...payload,
      email: payload.email?.trim() || null,
      phone: payload.phone?.trim() || null,
      address: payload.address?.trim() || null
    };

    return this.http.post<ClientDto>(this.clientsUrl, normalizedPayload).pipe(
      map((client) => ({
        ...client,
        email: client.email ?? null,
        phone: client.phone ?? null,
        address: client.address ?? null
      }))
    );
  }
}
