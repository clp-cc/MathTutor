import request from '@/utils/request'

export function Hello(data) {
    return request({
      url: '/test/hello',
      method: 'get'
    })
  }