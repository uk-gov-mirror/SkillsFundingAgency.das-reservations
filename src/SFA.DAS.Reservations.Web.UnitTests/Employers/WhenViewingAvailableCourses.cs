﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AutoFixture.NUnit3;
using FluentAssertions;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Moq;
using NUnit.Framework;
using SFA.DAS.Reservations.Application.Reservations.Queries.GetCourses;
using SFA.DAS.Reservations.Domain.Courses;
using SFA.DAS.Reservations.Web.Controllers;
using SFA.DAS.Reservations.Web.Models;

namespace SFA.DAS.Reservations.Web.UnitTests.Employers
{
    public class WhenViewingAvailableCourses
    {
        [Test, MoqAutoData]
        public async Task Then_All_Available_Courses_Are_Viewable(
            ICollection<Course> courses,
            [Frozen] Mock<IMediator> mockMediator,
            EmployerReservationsController controller)
        {
            //Assign
            mockMediator.Setup(m => m.Send(It.IsAny<GetCoursesQuery>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new GetCoursesResult
                {
                    Courses = courses
                });

            var expectedCourseViewModels = courses.Select(c => new CourseViewModel(c)).ToArray();

            //Act
            var result = await controller.SelectCourse(Guid.NewGuid()) as ViewResult;
            var viewModel = result?.Model as EmployerSelectCourseViewModel;

            //Assert
            Assert.IsNotNull(viewModel);
            expectedCourseViewModels.Should().BeEquivalentTo(viewModel.Courses);
        }

        [Test, MoqAutoData]
        public async Task Then_ReservationId_Will_Be_In_View_Model(
            ICollection<Course> courses,
            [Frozen] Mock<IMediator> mockMediator,
            EmployerReservationsController controller)
        {
            //Assign
            var expectedReservationId = Guid.NewGuid();
            mockMediator.Setup(m => m.Send(It.IsAny<GetCoursesQuery>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new GetCoursesResult
                {
                    Courses = courses
                });

            //Act
            var result = await controller.SelectCourse(expectedReservationId) as ViewResult;
            var viewModel = result?.Model as EmployerSelectCourseViewModel;

            //Assert
            Assert.IsNotNull(viewModel);
            Assert.AreEqual(expectedReservationId, viewModel.ReservationId);
        }
    }
}
