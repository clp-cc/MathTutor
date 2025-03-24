import request from '@/utils/request'

// export function login(data) {
//   return request({
//     url: '/WeatherForecast',
//     method: 'post',
//     data
//   })
// }

export function login(data) {
  return request({
    url: '/api/auth/login',
    method: 'post',
    data
  })
}

export function getInfo(token) {
  return request({
    url: '/api/auth/userinfo',
    method: 'get',
    params: { token }
  })
}

export function logout() {
  return request({
    url: '/api/user/logout',
    method: 'post'
  })
}
