import request from '@/utils/request'

export function createClass(data) {
  return request({
    url: '/api/class',
    method: 'post',
    data
  })
}

export function getMyClasses() {
  return request({
    url: '/api/class/my-classes',
    method: 'get'
  })
}

export function importStudents(classId, file) {
  const formData = new FormData()
  formData.append('file', file)
  return request({
    url: `/api/class/${classId}/ClassDetail/import`,
    method: 'post',
    data: formData,
    headers: {
      'Content-Type': 'multipart/form-data'
    }
  })
}

export function getClassDetail(classId, params) {
  return request({
    url: `/api/class/${classId}/ClassDetail`,
    method: 'get',
    params
  })
}

export function createStudent(classId, data) {
  return request({
    url: `/api/class/${classId}/ClassDetail`,
    method: 'post',
    data
  })
}

export function updateStudent(classId, userId,data) {
  return request({
    url: `/api/class/${classId}/ClassDetail/${userId}`,
    method: 'put',
    data
  })
}