<template>
    <div class="app-container">
        <div class="filter-container">
            <el-input v-model="listQuery.keyword" placeholder="输入学号或姓名搜索" style="width: 300px;" class="filter-item"
                @keyup.enter.native="getList" />

            <div class="op-container">
                <el-button type="primary" class="filter-item" @click="handleCreate">
                    添加成员
                </el-button>
                <el-button type="success" class="filter-item" @click="handleImport">
                    导入成员
                </el-button>
            </div>
        </div>

        <el-table v-loading="listLoading" :data="list" border fit highlight-current-row>
            <el-table-column align="center" label="学号" prop="studentNo" />
            <el-table-column align="center" label="姓名" prop="userName" />
            <el-table-column align="center" label="性别">
                <template slot-scope="{row}">
                    {{ row.gender | genderFilter }}
                </template>
            </el-table-column>
            <el-table-column align="center" label="手机号码" prop="phoneNumber" />
            <el-table-column align="center" label="状态">
                <template slot-scope="{row}">
                    <el-tag :type="row.status | statusFilter">
                        {{ row.status | statusTextFilter }}
                    </el-tag>
                </template>
            </el-table-column>
            <!-- <el-table-column 
          align="center" 
          label="操作" 
          width="120"
          v-if="$store.getters.roles.includes('teacher')"
        > -->
            <el-table-column align="center" label="操作" width="120">
                <template slot-scope="{row}">
                    <el-button type="primary" size="mini" icon="el-icon-edit" @click="handleUpdate(row)" />
                </template>
            </el-table-column>
        </el-table>

        <!-- 添加/编辑对话框 -->
        <el-dialog :title="textMap[dialogStatus]" :visible.sync="dialogFormVisible">
            <el-form ref="dataForm" :rules="rules" :model="temp" label-position="right" label-width="100px">
                <el-form-item label="学号" prop="studentNo">
                    <el-input v-model="temp.studentNo" />
                </el-form-item>
                <el-form-item label="姓名" prop="userName">
                    <el-input v-model="temp.userName" />
                </el-form-item>
                <el-form-item label="性别" prop="gender">
                    <el-select v-model="temp.gender">
                        <el-option label="男" :value="1" />
                        <el-option label="女" :value="0" />
                    </el-select>
                </el-form-item>
                <el-form-item label="手机号" prop="phoneNumber">
                    <el-input v-model="temp.phoneNumber" />
                </el-form-item>
            </el-form>
            <div slot="footer">
                <el-button @click="dialogFormVisible = false">取消</el-button>
                <el-button type="primary" @click="dialogStatus === 'create' ? createData() : updateData()">
                    确认
                </el-button>
            </div>
        </el-dialog>

        <!-- 导入对话框 -->
        <el-dialog title="导入成员" :visible.sync="importDialogVisible">
            <el-upload class="upload-demo" :auto-upload="false" :show-file-list="false" :on-change="handleFileChange">
                <el-button size="small" type="primary">点击上传</el-button>
            </el-upload>
            <div v-if="selectedFile" style="margin-top:15px;">
                <div style="margin-bottom:10px;">
                    已选择文件：{{ selectedFile.name }}
                </div>
                <el-button type="success" size="small" :loading="importLoading" @click="submitImport">
                    {{ importLoading ? '导入中...' : '开始导入' }}
                </el-button>
            </div>


            <div class="el-upload__tip" style="margin-top:10px;">
                <div>请按以下要求准备导入文件：</div>
                <div>1. 文件格式：Excel（.xlsx）</div>
                <div>2. 包含字段：学号、姓名、性别（男/女）、手机号</div>
                <div>3. 下载模板：
                    <a href="/templates/student_import_template.xlsx" download="学生信息导入模板.xlsx"
                        style="color: #409EFF; text-decoration: underline;">
                        点击下载模板文件
                    </a>
                </div>
                <div style="color: #999; margin-top: 5px;">
                    （如果无法下载，请右键链接另存为）
                </div>
            </div>
        </el-dialog>
    </div>
</template>
  
<script>
import { getClassDetail, createStudent, updateStudent, importStudents } from '@/api/class'
import { getToken } from '@/utils/auth'

export default {
    name: 'ClassDetail',
    filters: {
        genderFilter(gender) {
            return gender === 1 ? '男' : '女'
        },
        statusFilter(status) {
            return status === 0 ? 'danger' : 'success'
        },
        statusTextFilter(status) {
            return status === 0 ? '未激活' : '已激活'
        }
    },
    props: {
        classId: {
            type: [String, Number],
            required: true
        }
    },
    data() {
        return {
            list: [],
            listLoading: true,
            listQuery: {
                keyword: '',
                page: 1,
                limit: 20
            },
            total: 0,
            temp: {
                userId: '',
                studentNo: '',
                userName: '',
                gender: 1,
                phoneNumber: ''
            },
            dialogFormVisible: false,
            importDialogVisible: false,

            importLoading: false,
            selectedFile: null,
            dialogStatus: '',
            textMap: {
                update: '编辑成员',
                create: '添加成员'
            },
            rules: {
                studentNo: [{ required: true, message: '学号不能为空', trigger: 'blur' }],
                userName: [{ required: true, message: '姓名不能为空', trigger: 'blur' }],
                phoneNumber: [
                    { required: true, message: '手机号不能为空', trigger: 'blur' },
                    { pattern: /^1[3-9]\d{9}$/, message: '手机号格式不正确' }
                ]
            },
            headers: {
                Authorization: `Bearer ${getToken()}`
            }
        }
    },
    computed: {
        importUrl() {
            return `${process.env.VUE_APP_BASE_API}/api/class/${this.classId}/ClassDetail/import`
        }
    },
    created() {
        this.getList()
    },
    methods: {
        async getList() {
            this.listLoading = true
            try {
                const params = {
                    keyword: this.listQuery.keyword // 搜索关键词参数
                };
                const res = await getClassDetail(this.classId, params)
                if (res.code === 20000) {
                    this.list = res.data.map(item => ({
                        ...item,
                        userId: item.userId,
                        studentNo: item.studentNo,
                        userName: item.userName
                    }))
                }
            } finally {
                this.listLoading = false
            }
        },
        handleCreate() {
            this.temp = {
                userId: '',
                studentNo: '',
                userName: '',
                gender: 1,
                phoneNumber: ''
            }
            this.dialogStatus = 'create'
            this.dialogFormVisible = true
            this.$nextTick(() => {
                this.$refs.dataForm.clearValidate()
            })
        },
        handleUpdate(row) {
            this.temp = { ...row }
            this.dialogStatus = 'update'
            this.dialogFormVisible = true
        },
        async createData() {
            this.$refs.dataForm.validate(async valid => {
                if (valid) {
                    try {
                        await createStudent(this.classId, this.temp)
                        this.getList()
                        this.dialogFormVisible = false
                        this.$message.success('添加成功')
                    } catch (error) {
                        console.error(error)
                    }
                }
            })
        },
        async updateData() {
            this.$refs.dataForm.validate(async valid => {
                if (valid) {
                    try {
                        await updateStudent(this.classId, this.temp.userId, this.temp)
                        this.getList()
                        this.dialogFormVisible = false
                        this.$message.success('修改成功')
                    } catch (error) {
                        console.error(error)
                    }
                }
            })
        },

        handleImport() {
            this.importDialogVisible = true
            this.selectedFile = null // 重置已选文件
        },
        handleFileChange(file) {
            this.selectedFile = file.raw
        },
        async submitImport() {
            if (!this.selectedFile) {
                this.$message.warning('请先选择文件')
                return
            }

            try {
                this.importLoading = true
                const res = await importStudents(this.classId, this.selectedFile)
                if (res.code === 20000) {
                    this.$message.success('导入成功')
                    this.getList()
                    this.importDialogVisible = false
                }
            } catch (error) {
                this.$message.error(`导入失败：${error.message || '未知错误'}`)
            } finally {
                this.importLoading = false
            }
        }
    }
}
</script>
  
<style scoped>
.filter-container {
    margin-bottom: 20px;
    display: flex;
    justify-content: space-between;
    align-items: center;
}

.op-container {
    display: flex;
    gap: 10px;
}
</style>