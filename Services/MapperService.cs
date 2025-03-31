using AutoMapper;
using Data;
using Models.Admin;
using Models.Authors;
using Models.Books;
using Models.Orders;
using Models.Users;

namespace Services
{
    public class MapperService : Profile
    {
        public MapperService()
        {
            CreateMap<BookModel, Book>().ReverseMap();

            CreateMap<AuthorModel, Author>().ReverseMap();

            CreateMap<OrderModel, Order>().ReverseMap();

            CreateMap<BannerImageModel, BannerImage>().ReverseMap();

            CreateMap<BookCommentModel, BookComment>().ReverseMap();

            CreateMap<FilterOptionsModel, FilterOptions>().ReverseMap();

            CreateMap<UserModel, User>().ReverseMap();

            CreateMap<LoginModel, User>().ReverseMap();

            CreateMap<RegistrationModel, User>().ReverseMap();

            CreateMap<MyProfileModel, User>().ReverseMap();

            CreateMap<CartItem, CartItemModel>()
                .ForMember(dest => dest.Book, opt => opt.MapFrom(src => new BookModel { Id = src.BookId }))
                .ReverseMap()
                .ForMember(dest => dest.BookId, opt => opt.MapFrom(src => src.Book.Id));
        }
    }
}
