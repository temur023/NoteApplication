import api from './api';

export interface Reminder {
  id: number;
  noteId: number;
  reminderTime: string;
}

export interface ReminderCreateDto {
  noteId: number;
  reminderTime: string;
}

export interface ReminderUpdateDto {
  id: number;
  reminderTime: string;
}

export interface ReminderFilter {
  page?: number;
  pageSize?: number;
}

export interface PagedResponse<T> {
  statusCode: number;
  message: string;
  data: T[];
  pageNumber?: number;
  pageSize?: number;
  totalRecords?: number;
}

export const remindersApi = {
  getAll: async (filter?: ReminderFilter): Promise<PagedResponse<Reminder>> => {
    const response = await api.get<PagedResponse<Reminder>>('/reminder/get-all', { params: filter });
    // Backend returns PagedResponse which extends Response<List<T>>
    if (Array.isArray(response.data.data)) {
      return {
        statusCode: response.data.statusCode || 200,
        message: response.data.message || '',
        data: response.data.data,
        pageNumber: response.data.pageNumber,
        pageSize: response.data.pageSize,
        totalRecords: response.data.totalRecords,
      };
    }
    return response.data;
  },

  getById: async (id: number): Promise<Reminder> => {
    const response = await api.get<{ statusCode: number; message: string; data: Reminder }>(`/reminder/${id}`);
    return response.data.data || response.data as any;
  },

  create: async (reminder: ReminderCreateDto): Promise<Reminder> => {
    const response = await api.post<{ statusCode: number; message: string; data: Reminder }>('/reminder/create', reminder);
    return response.data.data || response.data as any;
  },

  update: async (reminder: ReminderUpdateDto): Promise<Reminder> => {
    const response = await api.put<{ statusCode: number; message: string; data: Reminder }>('/reminder/update', reminder);
    return response.data.data || response.data as any;
  },

  delete: async (id: number): Promise<void> => {
    await api.delete(`/reminder/${id}`);
  },
};

